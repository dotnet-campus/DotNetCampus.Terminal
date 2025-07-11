using DotNetCampus.Terminal.FileSync.Models;
using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.FileSync;

/// <summary>
/// 文件同步结果
/// </summary>
public enum FileSyncResult
{
    /// <summary>
    /// 同步成功
    /// </summary>
    Success,

    /// <summary>
    /// 同步失败
    /// </summary>
    Failed,

    /// <summary>
    /// 同步被取消
    /// </summary>
    Cancelled,

    /// <summary>
    /// 同步部分成功（部分文件同步成功，部分失败）
    /// </summary>
    PartialSuccess,
}

/// <summary>
/// 文件同步进度信息
/// </summary>
public record FileSyncProgress
{
    /// <summary>
    /// 当前处理的文件路径
    /// </summary>
    public required string CurrentFile { get; init; }

    /// <summary>
    /// 当前文件的进度百分比 (0-100)
    /// </summary>
    public double CurrentFileProgress { get; init; }

    /// <summary>
    /// 总体进度百分比 (0-100)
    /// </summary>
    public double TotalProgress { get; init; }

    /// <summary>
    /// 已处理的文件数
    /// </summary>
    public int ProcessedFiles { get; init; }

    /// <summary>
    /// 总文件数
    /// </summary>
    public int TotalFiles { get; init; }
}

/// <summary>
/// 文件同步服务接口
/// </summary>
public interface IFileSyncService
{
    /// <summary>
    /// 同步单个目录
    /// </summary>
    /// <param name="sshInfo">SSH连接信息</param>
    /// <param name="syncingModel">目录同步配置</param>
    /// <param name="progressCallback">进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含详细错误信息的同步结果</returns>
    Task<SyncResult<int>> SyncDirectoryAsync(
        SshRemoteDeviceInfo sshInfo,
        DirectorySyncingModel syncingModel,
        IProgress<FileSyncProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 同步多个目录
    /// </summary>
    /// <param name="sshInfo">SSH连接信息</param>
    /// <param name="syncingModels">目录同步配置列表</param>
    /// <param name="progressCallback">进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含详细错误信息的多组同步结果</returns>
    Task<MultiSyncResult> SyncMultipleDirectoriesAsync(
        SshRemoteDeviceInfo sshInfo,
        IEnumerable<DirectorySyncingModel> syncingModels,
        IProgress<FileSyncProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);
}
