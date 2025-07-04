namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 描述服务的生命周期。
/// </summary>
public enum ServiceLifetime
{
    /// <summary>
    /// 此服务在服务集合中以单例形式存在。
    /// </summary>
    Singleton,

    /// <summary>
    /// 在每个请求的作用域内，此服务只会创建一个实例。
    /// </summary>
    Scoped,

    /// <summary>
    /// 当调用方每次向容器内请求服务时，都会创建一个新的服务实例。
    /// </summary>
    Transient,
}
