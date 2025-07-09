using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync.Core;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using Renci.SshNet;

namespace DotNetCampus.Terminal.FileSync.Operations;

/// <summary>
/// 远程到本地同步操作
/// </summary>
public class RemoteToLocalSyncOperation
{
    private readonly LocalFileInfoProvider _localFileProvider;
    private readonly RemoteFileInfoProvider _remoteFileProvider;
    private readonly IncrementalSyncComparator _syncComparator;

    public RemoteToLocalSyncOperation()
    {
        _localFileProvider = new LocalFileInfoProvider();
        _remoteFileProvider = new RemoteFileInfoProvider();
        _syncComparator = new IncrementalSyncComparator();
    }

    /// <summary>
    /// 执行远程到本地同步
    /// </summary>
    public Task<FileSyncResult> ExecuteAsync(
        SshRemoteDeviceInfo sshInfo,
        SyncGroupConfiguration syncGroup,
        IProgress<FileSyncProgress>? progressCallback,
        CancellationToken cancellationToken)
    {
        // 确保本地目录存在
        if (!Directory.Exists(syncGroup.LocalPath))
        {
            try
            {
                Directory.CreateDirectory(syncGroup.LocalPath);
                Log.Info($"[FileSync] 创建本地目录: {syncGroup.LocalPath}");
            }
            catch (Exception ex)
            {
                Log.Error($"[FileSync] 无法创建本地目录: {syncGroup.LocalPath}. 错误: {ex.Message}");
                return Task.FromResult(FileSyncResult.Failed);
            }
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

            // 检查远程目录是否存在
            if (!client.Exists(syncGroup.RemotePath))
            {
                Log.Error($"[FileSync] 远程目录不存在: {syncGroup.RemotePath}");
                return Task.FromResult(FileSyncResult.Failed);
            }

            // 获取本地和远程文件信息用于增量比较
            var localFileInfos = _localFileProvider.GetFileInfos(syncGroup.LocalPath);
            var remoteFileInfos = _remoteFileProvider.GetFileInfos(client, syncGroup.RemotePath);
            
            // 找出需要同步的文件（新文件或已修改的文件）
            var filesToSync = _syncComparator.GetFilesToSyncRemoteToLocal(remoteFileInfos, localFileInfos, syncGroup.RemotePath, syncGroup.LocalPath);
            int totalFiles = filesToSync.Count;
            int processedFiles = 0;

            if (totalFiles == 0)
            {
                Log.Info($"[FileSync] 没有文件需要同步: {syncGroup.Name}");
                return Task.FromResult(FileSyncResult.Success);
            }

            Log.Info($"[FileSync] 找到 {totalFiles} 个文件需要下载（增量同步）");

            // 执行文件同步
            foreach (var remoteFile in filesToSync)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Log.Warn("[FileSync] 文件同步操作被取消");
                    return Task.FromResult(FileSyncResult.Cancelled);
                }

                try
                {
                    // 计算相对路径并确定本地路径
                    string relativePath = remoteFile.Substring(syncGroup.RemotePath.Length).TrimStart('/');
                    string localFilePath = Path.Combine(syncGroup.LocalPath, relativePath.Replace('/', '\\'));

                    // 确保本地目录存在
                    string localDirectory = Path.GetDirectoryName(localFilePath) ?? syncGroup.LocalPath;
                    if (!Directory.Exists(localDirectory))
                    {
                        Directory.CreateDirectory(localDirectory);
                        Log.Debug($"[FileSync] 创建本地目录: {localDirectory}");
                    }

                    // 下载文件
                    Log.Debug($"[FileSync] 正在下载文件: {remoteFile} -> {localFilePath}");

                    using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                    {
                        // 获取远程文件信息用于进度计算
                        var fileInfo = client.Get(remoteFile);
                        long fileSize = fileInfo.Length;

                        client.DownloadFile(remoteFile, fileStream, progress =>
                        {
                            var currentProgress = fileSize > 0 ? (double)progress / fileSize * 100 : 100;
                            var totalProgress = ((double)processedFiles / totalFiles * 100) + (currentProgress / totalFiles);

                            progressCallback?.Report(new FileSyncProgress
                            {
                                CurrentFile = remoteFile,
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
                        CurrentFile = remoteFile,
                        CurrentFileProgress = 100,
                        TotalProgress = overallProgress,
                        ProcessedFiles = processedFiles,
                        TotalFiles = totalFiles
                    });
                }
                catch (Exception ex)
                {
                    Log.Error($"[FileSync] 下载文件时发生错误: {remoteFile}. 错误: {ex.Message}");
                    return Task.FromResult(FileSyncResult.Failed);
                }
            }

            Log.Info($"[FileSync] 远程到本地同步完成: {syncGroup.Name}");
            return Task.FromResult(FileSyncResult.Success);
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 执行远程到本地同步时发生错误: {syncGroup.Name}. 错误: {ex.Message}");
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
}
