namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 从容器中创建服务实例时，如果服务实例实现了此接口，则可以通过此接口获得创建此实例的服务提供者。<br />
/// 通过此服务提供者，可以获取到同组的其他服务实例，或者获取到单例服务。
/// </summary>
public interface IServiceProviderAware
{
    /// <summary>
    /// 创建了此服务实例的服务提供者。可以通过此服务提供者获取到同组的其他服务实例，或者获取到单例服务。
    /// </summary>
    IServiceProvider ServiceProvider { get; }
}
