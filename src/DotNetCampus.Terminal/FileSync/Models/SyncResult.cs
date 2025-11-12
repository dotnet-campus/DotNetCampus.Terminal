using System.Diagnostics.CodeAnalysis;

namespace DotNetCampus.Terminal.FileSync.Models;

/// <summary>
/// 文件同步结果包装器，支持携带详细错误信息
/// </summary>
/// <typeparam name="T">成功时的数据类型</typeparam>
public class SyncResult<T>
{
    private readonly T? _value;
    private readonly SyncError? _error;

    private SyncResult(T value)
    {
        _value = value;
        _error = null;
        IsSuccess = true;
    }

    private SyncResult(SyncError error)
    {
        _value = default;
        _error = error;
        IsSuccess = false;
    }

    /// <summary>
    /// 是否成功
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// 成功时的值
    /// </summary>
    public T? Value => _value;

    /// <summary>
    /// 失败时的错误信息
    /// </summary>
    public SyncError? Error => _error;

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static SyncResult<T> Success(T value) => new(value);

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static SyncResult<T> Failure(SyncError error) => new(error);

    /// <summary>
    /// 创建失败结果（基于异常）
    /// </summary>
    public static SyncResult<T> Failure(Exception exception, string operation = "", string? context = null)
    {
        return new(SyncError.FromException(exception, operation, context));
    }

    /// <summary>
    /// 创建失败结果（自定义错误）
    /// </summary>
    public static SyncResult<T> Failure(string message, SyncErrorType errorType = SyncErrorType.Unknown, string? context = null)
    {
        return new(new SyncError(message, errorType, context));
    }

    /// <summary>
    /// 创建取消结果
    /// </summary>
    public static SyncResult<T> Cancelled(string? reason = null)
    {
        return new(new SyncError(reason ?? "操作被取消", SyncErrorType.Cancelled));
    }
}

/// <summary>
/// 不带值的同步结果
/// </summary>
public class SyncResult
{
    private readonly bool _success;
    private readonly SyncError? _error;

    private SyncResult(bool success)
    {
        _success = success;
        _error = null;
        IsSuccess = success;
    }

    private SyncResult(SyncError error)
    {
        _success = false;
        _error = error;
        IsSuccess = false;
    }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 失败时的错误信息
    /// </summary>
    public SyncError? Error => _error;

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static SyncResult Success() => new(true);

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static SyncResult Failure(SyncError error) => new(error);

    /// <summary>
    /// 创建失败结果（基于异常）
    /// </summary>
    public static SyncResult Failure(Exception exception, string operation = "", string? context = null)
    {
        return new(SyncError.FromException(exception, operation, context));
    }

    /// <summary>
    /// 创建失败结果（自定义错误）
    /// </summary>
    public static SyncResult Failure(string message, SyncErrorType errorType = SyncErrorType.Unknown, string? context = null)
    {
        return new(new SyncError(message, errorType, context));
    }

    /// <summary>
    /// 创建取消结果
    /// </summary>
    public static SyncResult Cancelled(string? reason = null)
    {
        return new(new SyncError(reason ?? "操作被取消", SyncErrorType.Cancelled));
    }
}
