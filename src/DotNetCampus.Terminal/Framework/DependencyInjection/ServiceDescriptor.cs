using System.Diagnostics;

namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 依赖注入容器内服务的描述符。
/// </summary>
[DebuggerDisplay("{Lifetime} {ServiceType}")]
public record ServiceDescriptor
{
    /// <summary>
    /// 初始化一个服务 <see cref="ServiceDescriptor"/> 类的新实例。
    /// </summary>
    /// <param name="serviceType">服务的类型。</param>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    public ServiceDescriptor(Type serviceType, Func<IInitializingServiceProvider, object> implementationFactory)
    {
        ServiceType = serviceType;
        ImplementationFactory = (s, k) => implementationFactory(s);
    }

    /// <summary>
    /// 初始化一个命名服务 <see cref="ServiceDescriptor"/> 类的新实例。
    /// </summary>
    /// <param name="serviceType">服务的类型。</param>
    /// <param name="serviceKey">命名服务的名称。</param>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    public ServiceDescriptor(Type serviceType, string serviceKey, Func<IInitializingServiceProvider, string, object> implementationFactory)
    {
        ServiceType = serviceType;
        ServiceKey = serviceKey;
        ImplementationFactory = (s, k) => implementationFactory(s, k!);
    }

    /// <summary>
    /// 此服务的生命周期。
    /// </summary>
    public ServiceLifetime Lifetime { get; init; }

    /// <summary>
    /// 如果此服务是一个命名服务，则此属性表示服务的名称。
    /// </summary>
    public string? ServiceKey { get; }

    /// <summary>
    /// 此服务的类型。
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// 此服务的实现工厂。
    /// </summary>
    public Func<IInitializingServiceProvider, string?, object> ImplementationFactory { get; }

    /// <summary>
    /// 获取一个值，指示此服务是否是一个命名服务。
    /// </summary>
    public bool IsKeyedService => ServiceKey is not null;
}
