namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 依赖注入异常的基类。
/// </summary>
public abstract class IocException : Exception
{
    /// <summary>
    /// 初始化 <see cref="IocException"/> 类的新实例。
    /// </summary>
    protected IocException()
    {
    }

    /// <summary>
    /// 初始化 <see cref="IocException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    protected IocException(string? message) : base(message)
    {
    }

    /// <summary>
    /// 初始化 <see cref="IocException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息。</param>
    /// <param name="innerException">内部异常。</param>
    protected IocException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// 当作用域服务引用未被垃圾回收时抛出此异常。
/// </summary>
public sealed class ScopeServiceReferenceNotGarbageCollectedException : IocException
{
    /// <summary>
    /// 获取未被垃圾回收的引用列表。
    /// </summary>
    public IReadOnlyList<object> NotGarbageCollectedReferences { get; }

    /// <summary>
    /// 初始化 <see cref="ScopeServiceReferenceNotGarbageCollectedException"/> 类的新实例。
    /// </summary>
    public ScopeServiceReferenceNotGarbageCollectedException()
    {
        NotGarbageCollectedReferences = [];
    }

    /// <summary>
    /// 初始化 <see cref="ScopeServiceReferenceNotGarbageCollectedException"/> 类的新实例。
    /// </summary>
    /// <param name="notGarbageCollectedReferences">未被垃圾回收的引用列表。</param>
    /// <param name="message">异常消息。</param>
    public ScopeServiceReferenceNotGarbageCollectedException(IReadOnlyList<object> notGarbageCollectedReferences, string? message) : base(message)
    {
        NotGarbageCollectedReferences = notGarbageCollectedReferences;
    }
}
