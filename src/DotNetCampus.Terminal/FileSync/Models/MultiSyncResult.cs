using DotNetCampus.Terminal.FileSync.Models;

namespace DotNetCampus.Terminal.FileSync.Models;

/// <summary>
/// 多个目录同步的同步结果
/// </summary>
public class MultiSyncResult
{
    /// <summary>
    /// 个别目录同步的结果
    /// </summary>
    public required List<GroupSyncResult> GroupResults { get; init; }

    /// <summary>
    /// 整体同步状态
    /// </summary>
    public FileSyncResult OverallResult { get; init; }

    /// <summary>
    /// 是否有任何错误
    /// </summary>
    public bool HasErrors => GroupResults.Any(r => !r.IsSuccess);

    /// <summary>
    /// 是否所有同步都成功
    /// </summary>
    public bool IsSuccess => GroupResults.All(r => r.IsSuccess);

    /// <summary>
    /// 获取所有错误信息
    /// </summary>
    public List<SyncError> GetAllErrors()
    {
        return GroupResults
            .Where(r => !r.IsSuccess && r.Error != null)
            .Select(r => r.Error!)
            .ToList();
    }

    /// <summary>
    /// 获取用户友好的错误摘要
    /// </summary>
    public string GetErrorSummary()
    {
        var errors = GetAllErrors();
        if (errors.Count == 0)
            return string.Empty;

        if (errors.Count == 1)
            return errors[0].GetBriefMessage();

        var errorsByType = errors.GroupBy(e => e.ErrorType);
        var summaryLines = new List<string>();

        foreach (var group in errorsByType)
        {
            var count = group.Count();
            var typeName = group.Key switch
            {
                SyncErrorType.NetworkError => "网络错误",
                SyncErrorType.AuthenticationError => "认证错误",
                SyncErrorType.FileSystemError => "文件系统错误",
                SyncErrorType.RemotePathNotFound => "远程路径错误",
                SyncErrorType.LocalPathError => "本地路径错误",
                SyncErrorType.ConfigurationError => "配置错误",
                SyncErrorType.TransferError => "传输错误",
                _ => "其他错误",
            };

            summaryLines.Add($"{typeName} {count}个");
        }

        return string.Join("，", summaryLines);
    }

    /// <summary>
    /// 获取详细的诊断信息
    /// </summary>
    public string GetDetailedDiagnostics()
    {
        var diagnostics = new List<string>();

        diagnostics.Add($"总体结果: {OverallResult}");
        diagnostics.Add($"目录同步总数: {GroupResults.Count}");
        diagnostics.Add($"成功: {GroupResults.Count(r => r.IsSuccess)}");
        diagnostics.Add($"失败: {GroupResults.Count(r => !r.IsSuccess)}");
        diagnostics.Add("");

        foreach (var groupResult in GroupResults.Where(r => !r.IsSuccess))
        {
            diagnostics.Add($"目录同步: {groupResult.GroupName}");
            if (groupResult.Error != null)
            {
                diagnostics.Add(groupResult.Error.GetDiagnosticInfo());
            }
            diagnostics.Add("");
        }

        return string.Join(Environment.NewLine, diagnostics);
    }
}

/// <summary>
/// 单个目录同步的结果
/// </summary>
public record GroupSyncResult
{
    /// <summary>
    /// 目录同步名称
    /// </summary>
    public required string GroupName { get; init; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 错误信息（失败时）
    /// </summary>
    public SyncError? Error { get; init; }

    /// <summary>
    /// 同步的文件数量
    /// </summary>
    public int SyncedFileCount { get; init; }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static GroupSyncResult Success(string groupName, int syncedFileCount = 0)
    {
        return new GroupSyncResult
        {
            GroupName = groupName,
            IsSuccess = true,
            SyncedFileCount = syncedFileCount,
        };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static GroupSyncResult Failure(string groupName, SyncError error)
    {
        return new GroupSyncResult
        {
            GroupName = groupName,
            IsSuccess = false,
            Error = error,
        };
    }
}
