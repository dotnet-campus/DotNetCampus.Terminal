using DotNetCampus.Logging;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using Renci.SshNet;

namespace DotNetCampus.Terminal.FileSync;

/// <summary>
/// 基于SSH.NET的文件同步服务实现
/// </summary>
public class FileSyncService : IFileSyncService
{
    /// <inheritdoc />
    public async Task<FileSyncResult> SyncDirectoryAsync(
        SshRemoteDeviceInfo sshInfo,
        SyncGroupConfiguration syncGroup,
        IProgress<FileSyncProgress>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(syncGroup.LocalPath) || string.IsNullOrEmpty(syncGroup.RemotePath))
        {
            Log.Error($"[FileSync] 同步组 {syncGroup.Name} 的本地路径或远程路径为空");
            return FileSyncResult.Failed;
        }

        if (!syncGroup.Enabled)
        {
            Log.Info($"[FileSync] 同步组 {syncGroup.Name} 已被禁用，跳过同步");
            return FileSyncResult.Cancelled;
        }

        Log.Info($"[FileSync] 开始同步目录 {syncGroup.Name}: {syncGroup.LocalPath} -> {syncGroup.RemotePath}");

        try
        {
            // 使用Task.Run包装同步操作以避免UI线程阻塞
            return await Task.Run(() => SyncDirectoryInternalAsync(
                sshInfo, syncGroup, progressCallback, cancellationToken), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Log.Warn($"[FileSync] 同步操作被取消: {syncGroup.Name}");
            return FileSyncResult.Cancelled;
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 同步目录时发生错误: {syncGroup.Name}. 错误: {ex.Message}");
            return FileSyncResult.Failed;
        }
    }

    /// <inheritdoc />
    public async Task<FileSyncResult> SyncMultipleDirectoriesAsync(
        SshRemoteDeviceInfo sshInfo,
        IEnumerable<SyncGroupConfiguration> syncGroups,
        IProgress<FileSyncProgress>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var enabledSyncGroups = syncGroups.Where(sg => sg.Enabled).ToList();
        if (enabledSyncGroups.Count == 0)
        {
            Log.Info("[FileSync] 没有启用的同步组，跳过同步");
            return FileSyncResult.Cancelled;
        }

        Log.Info($"[FileSync] 开始同步多个目录，共 {enabledSyncGroups.Count} 个同步组");

        var results = new List<FileSyncResult>();

        foreach (var syncGroup in enabledSyncGroups)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Log.Warn("[FileSync] 多目录同步操作被取消");
                return FileSyncResult.Cancelled;
            }

            var result = await SyncDirectoryAsync(sshInfo, syncGroup, progressCallback, cancellationToken);
            results.Add(result);
        }

        // 判断整体同步结果
        if (results.All(r => r == FileSyncResult.Success))
        {
            return FileSyncResult.Success;
        }
        else if (results.Any(r => r == FileSyncResult.Success))
        {
            return FileSyncResult.PartialSuccess;
        }
        else if (results.All(r => r == FileSyncResult.Cancelled))
        {
            return FileSyncResult.Cancelled;
        }
        else
        {
            return FileSyncResult.Failed;
        }
    }

    private Task<FileSyncResult> SyncDirectoryInternalAsync(
        SshRemoteDeviceInfo sshInfo,
        SyncGroupConfiguration syncGroup,
        IProgress<FileSyncProgress>? progressCallback,
        CancellationToken cancellationToken)
    {
        // 确保本地目录存在
        if (!Directory.Exists(syncGroup.LocalPath))
        {
            Log.Error($"[FileSync] 本地目录不存在: {syncGroup.LocalPath}");
            return Task.FromResult(FileSyncResult.Failed);
        }

        // 创建SSH客户端
        using var client = new SftpClient(sshInfo.Host, sshInfo.Port, sshInfo.UserName, sshInfo.Password ?? string.Empty);

        try
        {
            Log.Info($"[FileSync] 正在连接到 {sshInfo.Host}:{sshInfo.Port}");
            client.Connect();

            if (!client.IsConnected)
            {
                Log.Error($"[FileSync] 无法连接到服务器: {sshInfo.Host}:{sshInfo.Port}");
                return Task.FromResult(FileSyncResult.Failed);
            }

            // 确保远程目录存在
            EnsureRemoteDirectoryExists(client, syncGroup.RemotePath);

            // 获取要同步的文件列表
            var localFiles = GetLocalFiles(syncGroup.LocalPath);
            int totalFiles = localFiles.Count;
            int processedFiles = 0;

            Log.Info($"[FileSync] 找到 {totalFiles} 个文件需要同步");

            // 执行文件同步
            foreach (var localFile in localFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Log.Warn("[FileSync] 文件同步操作被取消");
                    return Task.FromResult(FileSyncResult.Cancelled);
                }

                try
                {
                    // 计算相对路径并确定远程路径
                    string relativePath = localFile.Substring(syncGroup.LocalPath.Length).TrimStart('\\', '/');
                    string remoteFilePath = Path.Combine(syncGroup.RemotePath, relativePath).Replace('\\', '/');

                    // 确保远程目录存在
                    string remoteDirectory = Path.GetDirectoryName(remoteFilePath)?.Replace('\\', '/') ?? syncGroup.RemotePath;
                    EnsureRemoteDirectoryExists(client, remoteDirectory);

                    // 上传文件
                    Log.Debug($"[FileSync] 正在上传文件: {localFile} -> {remoteFilePath}");

                    using (var fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
                    {
                        client.UploadFile(fileStream, remoteFilePath, true, progress =>
                        {
                            var currentProgress = (double)progress / fileStream.Length * 100;
                            var totalProgress = ((double)processedFiles / totalFiles * 100) + (currentProgress / totalFiles);

                            progressCallback?.Report(new FileSyncProgress
                            {
                                CurrentFile = localFile,
                                CurrentFileProgress = currentProgress,
                                TotalProgress = totalProgress,
                                ProcessedFiles = processedFiles,
                                TotalFiles = totalFiles
                            });
                        });
                    }

                    processedFiles++;

                    // 报告总体进度
                    double overallProgress = (double)processedFiles / totalFiles * 100;
                    progressCallback?.Report(new FileSyncProgress
                    {
                        CurrentFile = localFile,
                        CurrentFileProgress = 100,
                        TotalProgress = overallProgress,
                        ProcessedFiles = processedFiles,
                        TotalFiles = totalFiles
                    });
                }
                catch (Exception ex)
                {
                    Log.Error($"[FileSync] 上传文件时发生错误: {localFile}. 错误: {ex.Message}");
                    return Task.FromResult(FileSyncResult.Failed);
                }
            }

            Log.Info($"[FileSync] 目录同步完成: {syncGroup.Name}");
            return Task.FromResult(FileSyncResult.Success);
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 执行目录同步时发生错误: {syncGroup.Name}. 错误: {ex.Message}");
            return Task.FromResult(FileSyncResult.Failed);
        }
        finally
        {
            if (client.IsConnected)
            {
                Log.Debug($"[FileSync] 断开与服务器的连接: {sshInfo.Host}:{sshInfo.Port}");
                client.Disconnect();
            }
        }
    }

    private List<string> GetLocalFiles(string localPath)
    {
        var files = new List<string>();

        try
        {
            files.AddRange(Directory.GetFiles(localPath, "*", SearchOption.AllDirectories));
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 获取本地文件列表时发生错误: {localPath}. 错误: {ex.Message}");
        }

        return files;
    }

    private void EnsureRemoteDirectoryExists(SftpClient client, string remoteDirectory)
    {
        string[] directories = remoteDirectory.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string currentPath = "/";

        foreach (string directory in directories)
        {
            currentPath = Path.Combine(currentPath, directory).Replace('\\', '/');

            if (!client.Exists(currentPath))
            {
                Log.Debug($"[FileSync] 创建远程目录: {currentPath}");
                client.CreateDirectory(currentPath);
            }
        }
    }
}
