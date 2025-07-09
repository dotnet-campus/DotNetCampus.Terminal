using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync.Operations;
using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.FileSync;

/// <summary>
/// 基于SSH.NET的文件同步服务实现
/// </summary>
public class FileSyncService : IFileSyncService
{
    private readonly LocalToRemoteSyncOperation _localToRemoteSync;
    private readonly RemoteToLocalSyncOperation _remoteToLocalSync;

    public FileSyncService()
    {
        _localToRemoteSync = new LocalToRemoteSyncOperation();
        _remoteToLocalSync = new RemoteToLocalSyncOperation();
    }

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

        var directionText = syncGroup.DirectionEnum == SyncDirection.LocalToRemote 
            ? $"{syncGroup.LocalPath} -> {syncGroup.RemotePath}" 
            : $"{syncGroup.RemotePath} -> {syncGroup.LocalPath}";
        Log.Info($"[FileSync] 开始增量同步目录 {syncGroup.Name}: {directionText}");

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
        // 根据同步方向选择同步方法
        return syncGroup.DirectionEnum switch
        {
            SyncDirection.LocalToRemote => _localToRemoteSync.ExecuteAsync(sshInfo, syncGroup, progressCallback, cancellationToken),
            SyncDirection.RemoteToLocal => _remoteToLocalSync.ExecuteAsync(sshInfo, syncGroup, progressCallback, cancellationToken),
            _ => throw new ArgumentException($"不支持的同步方向: {syncGroup.DirectionEnum}")
        };
    }
}
