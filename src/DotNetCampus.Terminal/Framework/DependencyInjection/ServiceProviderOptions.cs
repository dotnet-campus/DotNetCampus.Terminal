namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 为创建的服务提供者提供配置项。
/// </summary>
public record ServiceProviderOptions
{
    /// <summary>
    /// 服务提供者是否共享单例实例。
    /// <list type="bullet">
    /// <item><description>当设置为 <see langword="true"/> 时，所有同样设置为 <see langword="true"/> 的服务提供者（<see cref="IServiceProvider"/>）所提供的服务共享单例实例。</description></item>
    /// <item><description>当设置为 <see langword="false"/> 时，这个服务提供者内部提供单例实例而不会与其他服务提供者共享。</description></item>
    /// </list>
    /// </summary>
    public bool ShareSingletonInstances { get; init; }
}
