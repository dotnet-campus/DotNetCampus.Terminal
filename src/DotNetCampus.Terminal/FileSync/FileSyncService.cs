using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync.Models;
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
    public async Task<SyncResult<int>> SyncDirectoryAsync(
        SshRemoteDeviceInfo sshInfo,
        DirectorySyncingModel syncingModel,
        IProgress<FileSyncProgress>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(syncingModel.LocalPath) || string.IsNullOrEmpty(syncingModel.RemotePath))
        {
            var error = new SyncError(
                "目录同步的本地路径或远程路径为空",
                SyncErrorType.ConfigurationError,
                $"目录同步: {syncingModel.Name}");
            Log.Error($"[FileSync] {error.GetUserFriendlyMessage()}");
            return SyncResult<int>.Failure(error);
        }

        if (!syncingModel.Enabled)
        {
            Log.Info($"[FileSync] 目录同步 {syncingModel.Name} 已被禁用，跳过同步");
            return SyncResult<int>.Cancelled("目录同步已被禁用");
        }

        var directionText = syncingModel.GetDirection() == SyncDirection.LocalToRemote
            ? $"{syncingModel.LocalPath} -> {syncingModel.RemotePath}"
            : $"{syncingModel.RemotePath} -> {syncingModel.LocalPath}";
        Log.Info($"[FileSync] 开始增量同步目录 {syncingModel.Name}: {directionText}");

        try
        {
            // 使用Task.Run包装同步操作以避免UI线程阻塞
            return await Task.Run(() => SyncDirectoryInternalAsync(
                sshInfo, syncingModel, progressCallback, cancellationToken), cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            Log.Warn($"[FileSync] 同步操作被取消: {syncingModel.Name}");
            return SyncResult<int>.Cancelled($"同步操作被取消: {ex.Message}");
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 同步目录时发生错误: {syncingModel.Name}. 错误: {ex.Message}");
            return SyncResult<int>.Failure(ex, $"同步目录 {syncingModel.Name}", syncingModel.Name);
        }
    }

    /// <inheritdoc />
    public async Task<MultiSyncResult> SyncMultipleDirectoriesAsync(
        SshRemoteDeviceInfo sshInfo,
        IEnumerable<DirectorySyncingModel> syncingModels,
        IProgress<FileSyncProgress>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var enabledSyncingModels = syncingModels.Where(sg => sg.Enabled).ToList();
        if (enabledSyncingModels.Count == 0)
        {
            Log.Info("[FileSync] 没有启用的目录同步，跳过同步");
            return new MultiSyncResult
            {
                GroupResults = [],
                OverallResult = FileSyncResult.Cancelled,
            };
        }

        Log.Info($"[FileSync] 开始同步多个目录，共 {enabledSyncingModels.Count} 个目录同步");

        var groupResults = new List<GroupSyncResult>();

        foreach (var syncingModel in enabledSyncingModels)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Log.Warn("[FileSync] 多目录同步操作被取消");
                break;
            }

            var result = await SyncDirectoryAsync(sshInfo, syncingModel, progressCallback, cancellationToken);

            if (result.IsSuccess)
            {
                groupResults.Add(GroupSyncResult.Success(syncingModel.Name, result.Value));
            }
            else if (result.Error?.ErrorType == SyncErrorType.Cancelled)
            {
                groupResults.Add(GroupSyncResult.Failure(syncingModel.Name, result.Error));
                break; // 取消后不继续其他组
            }
            else
            {
                groupResults.Add(GroupSyncResult.Failure(syncingModel.Name, result.Error!));
            }
        }

        // 判断整体同步结果
        var overallResult = FileSyncResult.Failed;
        if (groupResults.All(r => r.IsSuccess))
        {
            overallResult = FileSyncResult.Success;
        }
        else if (groupResults.Any(r => r.IsSuccess))
        {
            overallResult = FileSyncResult.PartialSuccess;
        }
        else if (groupResults.Any(r => r.Error?.ErrorType == SyncErrorType.Cancelled))
        {
            overallResult = FileSyncResult.Cancelled;
        }

        return new MultiSyncResult
        {
            GroupResults = groupResults,
            OverallResult = overallResult,
        };
    }

    private Task<SyncResult<int>> SyncDirectoryInternalAsync(
        SshRemoteDeviceInfo sshInfo,
        DirectorySyncingModel syncingModel,
        IProgress<FileSyncProgress>? progressCallback,
        CancellationToken cancellationToken)
    {
        // 根据同步方向选择同步方法
        return syncingModel.GetDirection() switch
        {
            SyncDirection.LocalToRemote => _localToRemoteSync.ExecuteAsync(sshInfo, syncingModel, progressCallback, cancellationToken),
            SyncDirection.RemoteToLocal => _remoteToLocalSync.ExecuteAsync(sshInfo, syncingModel, progressCallback, cancellationToken),
            _ => Task.FromResult(SyncResult<int>.Failure($"不支持的同步方向: {syncingModel.GetDirection()}", SyncErrorType.ConfigurationError, syncingModel.Name)),
        };
    }
}
