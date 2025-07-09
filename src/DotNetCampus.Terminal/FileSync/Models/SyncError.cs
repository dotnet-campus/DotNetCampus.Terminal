namespace DotNetCampus.Terminal.FileSync.Models;

/// <summary>
/// 同步错误类型
/// </summary>
public enum SyncErrorType
{
    /// <summary>
    /// 未知错误
    /// </summary>
    Unknown,

    /// <summary>
    /// 网络连接错误
    /// </summary>
    NetworkError,

    /// <summary>
    /// 认证失败
    /// </summary>
    AuthenticationError,

    /// <summary>
    /// 文件系统错误（权限、磁盘空间等）
    /// </summary>
    FileSystemError,

    /// <summary>
    /// 远程路径不存在
    /// </summary>
    RemotePathNotFound,

    /// <summary>
    /// 本地路径不存在或无法创建
    /// </summary>
    LocalPathError,

    /// <summary>
    /// 配置错误（路径为空等）
    /// </summary>
    ConfigurationError,

    /// <summary>
    /// 操作被取消
    /// </summary>
    Cancelled,

    /// <summary>
    /// 文件传输错误
    /// </summary>
    TransferError
}

/// <summary>
/// 同步错误详细信息
/// </summary>
public record SyncError(
    string Message,
    SyncErrorType ErrorType = SyncErrorType.Unknown,
    string? Context = null)
{
    /// <summary>
    /// 内部异常信息
    /// </summary>
    public string? InnerException { get; init; }

    /// <summary>
    /// 错误发生的时间
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// 从异常创建错误信息
    /// </summary>
    public static SyncError FromException(Exception exception, string operation = "", string? context = null)
    {
        var errorType = exception switch
        {
            OperationCanceledException => SyncErrorType.Cancelled,
            UnauthorizedAccessException => SyncErrorType.AuthenticationError,
            DirectoryNotFoundException => SyncErrorType.LocalPathError,
            IOException => SyncErrorType.FileSystemError,
            System.Net.Sockets.SocketException => SyncErrorType.NetworkError,
            _ when exception.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) => SyncErrorType.AuthenticationError,
            _ when exception.Message.Contains("network", StringComparison.OrdinalIgnoreCase) => SyncErrorType.NetworkError,
            _ when exception.Message.Contains("path", StringComparison.OrdinalIgnoreCase) => SyncErrorType.LocalPathError,
            _ => SyncErrorType.Unknown
        };

        var message = string.IsNullOrEmpty(operation) 
            ? exception.Message 
            : $"{operation}: {exception.Message}";

        return new SyncError(message, errorType, context)
        {
            InnerException = exception.InnerException?.Message
        };
    }

    /// <summary>
    /// 获取用户友好的错误描述
    /// </summary>
    public string GetUserFriendlyMessage()
    {
        return ErrorType switch
        {
            SyncErrorType.NetworkError => $"网络连接失败: {Message}",
            SyncErrorType.AuthenticationError => $"身份验证失败: {Message}",
            SyncErrorType.FileSystemError => $"文件系统错误: {Message}",
            SyncErrorType.RemotePathNotFound => $"远程路径不存在: {Context ?? "未知路径"}",
            SyncErrorType.LocalPathError => $"本地路径错误: {Context ?? "未知路径"}",
            SyncErrorType.ConfigurationError => $"配置错误: {Message}",
            SyncErrorType.Cancelled => "同步操作被取消",
            SyncErrorType.TransferError => $"文件传输失败: {Message}",
            _ => $"同步失败: {Message}"
        };
    }

    /// <summary>
    /// 获取简洁的错误描述（用于消息拼接，不包含冒号）
    /// </summary>
    public string GetBriefMessage()
    {
        return ErrorType switch
        {
            SyncErrorType.NetworkError => $"网络连接失败({Message})",
            SyncErrorType.AuthenticationError => $"身份验证失败({Message})",
            SyncErrorType.FileSystemError => $"文件系统错误({Message})",
            SyncErrorType.RemotePathNotFound => $"远程路径不存在({Context ?? "未知路径"})",
            SyncErrorType.LocalPathError => $"本地路径错误({Context ?? "未知路径"})",
            SyncErrorType.ConfigurationError => $"配置错误({Message})",
            SyncErrorType.Cancelled => "操作被取消",
            SyncErrorType.TransferError => $"文件传输失败({Message})",
            _ => $"同步失败({Message})"
        };
    }

    /// <summary>
    /// 获取详细的诊断信息
    /// </summary>
    public string GetDiagnosticInfo()
    {
        var lines = new List<string>
        {
            $"错误类型: {ErrorType}",
            $"错误消息: {Message}",
            $"发生时间: {Timestamp:yyyy-MM-dd HH:mm:ss}"
        };

        if (!string.IsNullOrEmpty(Context))
        {
            lines.Add($"上下文: {Context}");
        }

        if (!string.IsNullOrEmpty(InnerException))
        {
            lines.Add($"内部异常: {InnerException}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
