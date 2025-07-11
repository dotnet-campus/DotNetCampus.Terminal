using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync.Core;
using DotNetCampus.Terminal.FileSync.Models;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using Renci.SshNet;

namespace DotNetCampus.Terminal.FileSync.Operations;

/// <summary>
/// 本地到远程同步操作
/// </summary>
public class LocalToRemoteSyncOperation
{
    private readonly LocalFileInfoProvider _localFileProvider;
    private readonly RemoteFileInfoProvider _remoteFileProvider;
    private readonly IncrementalSyncComparator _syncComparator;
    private readonly SftpOperationHelper _sftpHelper;

    public LocalToRemoteSyncOperation()
    {
        _localFileProvider = new LocalFileInfoProvider();
        _remoteFileProvider = new RemoteFileInfoProvider();
        _syncComparator = new IncrementalSyncComparator();
        _sftpHelper = new SftpOperationHelper();
    }

    /// <summary>
    /// 执行本地到远程同步
    /// </summary>
    public Task<SyncResult<int>> ExecuteAsync(
        SshRemoteDeviceInfo sshInfo,
        DirectorySyncingModel syncingModel,
        IProgress<FileSyncProgress>? progressCallback,
        CancellationToken cancellationToken)
    {
        // 确保本地目录存在
        if (!Directory.Exists(syncingModel.LocalPath))
        {
            var error = new SyncError(
                "本地目录不存在",
                SyncErrorType.LocalPathError,
                syncingModel.LocalPath);
            Log.Error($"[FileSync] {error.GetUserFriendlyMessage()}");
            return Task.FromResult(SyncResult<int>.Failure(error));
        }

        // 创建SSH客户端
        using var client = new SftpClient(sshInfo.Host, sshInfo.Port, sshInfo.UserName, sshInfo.Password ?? string.Empty);

        try
        {
            Log.Info($"[FileSync] 正在连接到 {sshInfo.Host}:{sshInfo.Port}");
            client.Connect();

            if (!client.IsConnected)
            {
                var error = new SyncError(
                    $"无法连接到服务器: {sshInfo.Host}:{sshInfo.Port}",
                    SyncErrorType.NetworkError,
                    $"{sshInfo.Host}:{sshInfo.Port}");
                Log.Error($"[FileSync] {error.GetUserFriendlyMessage()}");
                return Task.FromResult(SyncResult<int>.Failure(error));
            }

            // 确保远程目录存在
            try
            {
                _sftpHelper.EnsureRemoteDirectoryExists(client, syncingModel.RemotePath);
            }
            catch (Exception ex)
            {
                Log.Error($"[FileSync] 无法创建远程目录: {syncingModel.RemotePath}. 错误: {ex.Message}");
                return Task.FromResult(SyncResult<int>.Failure(ex, "创建远程目录", syncingModel.RemotePath));
            }

            // 获取本地和远程文件信息用于增量比较
            var localFileInfos = _localFileProvider.GetFileInfos(syncingModel.LocalPath);
            var remoteFileInfos = _remoteFileProvider.GetFileInfos(client, syncingModel.RemotePath);

            // 找出需要同步的文件（新文件或已修改的文件）
            var filesToSync = _syncComparator.GetFilesToSyncLocalToRemote(localFileInfos, remoteFileInfos, syncingModel.LocalPath, syncingModel.RemotePath);
            var totalFiles = filesToSync.Count;
            var processedFiles = 0;

            if (totalFiles == 0)
            {
                Log.Info($"[FileSync] 没有文件需要同步: {syncingModel.Name}");
                return Task.FromResult(SyncResult<int>.Success(0));
            }

            Log.Info($"[FileSync] 找到 {totalFiles} 个文件需要上传（增量同步）");

            // 执行文件同步
            foreach (var localFile in filesToSync)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Log.Warn("[FileSync] 文件同步操作被取消");
                    return Task.FromResult(SyncResult<int>.Cancelled("文件同步操作被取消"));
                }

                try
                {
                    // 计算相对路径并确定远程路径
                    var relativePath = Path.GetRelativePath(syncingModel.LocalPath, localFile);
                    var remoteFilePath = $"{syncingModel.RemotePath}/{relativePath.Replace('\\', '/')}";

                    // 确保远程目录存在
                    var remoteDirectory = Path.GetDirectoryName(remoteFilePath)?.Replace('\\', '/') ?? syncingModel.RemotePath;
                    _sftpHelper.EnsureRemoteDirectoryExists(client, remoteDirectory);

                    // 上传文件
                    Log.Debug($"[FileSync] 正在上传文件: {localFile} -> {remoteFilePath}");

                    using (var fileStream = File.OpenRead(localFile))
                    {
                        var fileSize = fileStream.Length;

                        client.UploadFile(fileStream, remoteFilePath, true, progress =>
                        {
                            var currentProgress = fileSize > 0 ? (double)progress / fileSize * 100 : 100;
                            var totalProgress = ((double)processedFiles / totalFiles * 100) + (currentProgress / totalFiles);

                            progressCallback?.Report(new FileSyncProgress
                            {
                                CurrentFile = localFile,
                                CurrentFileProgress = currentProgress,
                                TotalProgress = totalProgress,
                                ProcessedFiles = processedFiles,
                                TotalFiles = totalFiles,
                            });
                        });
                    }

                    processedFiles++;

                    // 报告总体进度
                    var overallProgress = (double)processedFiles / totalFiles * 100;
                    progressCallback?.Report(new FileSyncProgress
                    {
                        CurrentFile = localFile,
                        CurrentFileProgress = 100,
                        TotalProgress = overallProgress,
                        ProcessedFiles = processedFiles,
                        TotalFiles = totalFiles,
                    });
                }
                catch (Exception ex)
                {
                    Log.Error($"[FileSync] 上传文件时发生错误: {localFile}. 错误: {ex.Message}");
                    return Task.FromResult(SyncResult<int>.Failure(ex, "上传文件", localFile));
                }
            }

            Log.Info($"[FileSync] 本地到远程同步完成: {syncingModel.Name}");
            return Task.FromResult(SyncResult<int>.Success(processedFiles));
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 执行本地到远程同步时发生错误: {syncingModel.Name}. 错误: {ex.Message}");
            return Task.FromResult(SyncResult<int>.Failure(ex, "执行本地到远程同步", syncingModel.Name));
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
