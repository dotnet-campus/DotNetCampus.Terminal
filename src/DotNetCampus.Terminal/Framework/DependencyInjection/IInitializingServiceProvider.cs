namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 在依赖注入容器初始化过程中可供使用的服务提供者。
/// </summary>
public interface IInitializingServiceProvider : IServiceProvider
{
    // /// <summary>
    // /// 创建一个 <see cref="ServiceCreator{T}"/> 对象，用于稍后使用源生成器创建服务实例。
    // /// </summary>
    // /// <typeparam name="T">服务类型。</typeparam>
    // /// <returns></returns>
    // [Pure]
    // ServiceCreator<T> Create<T>() where T : notnull => new ServiceCreator<T>(this);

    /// <summary>
    /// 从现有的 <see cref="IServiceProvider"/> 实例获取或创建一个 <see cref="IInitializingServiceProvider"/> 实例。
    /// </summary>
    /// <param name="serviceProvider">要获取或创建的服务提供者。</param>
    /// <returns>专门用来初始化服务实例的服务提供者。</returns>
    internal static IInitializingServiceProvider From(IServiceProvider serviceProvider) => serviceProvider switch
    {
        IInitializingServiceProvider initializingServiceProvider => initializingServiceProvider,
        _ => new InitializingServiceProviderProxy(serviceProvider),
    };
}

/// <summary>
/// 可供源生成器使用的服务创建器。
/// </summary>
/// <param name="Locator">服务提供者。</param>
/// <typeparam name="T">服务类型。</typeparam>
public readonly record struct ServiceCreator<T>(IServiceProvider Locator)
{
    // /// <summary>
    // /// 使用依赖注入服务创建器创建 <typeparamref name="T"/> 的实例，并对其注入依赖。
    // /// </summary>
    // /// <returns>已注入依赖的 <typeparamref name="T"/> 实例。</returns>
    // public T Satisfy(MethodIsWaitingToBeGenerated _ = null)
    // {
    //     throw new InvalidOperationException("请使用源生成器生成的重载，而不是直接调用此方法。");
    // }

    /// <summary>
    /// 请不要实例化此类型。此类型仅供源生成器使用。
    /// </summary>
    public sealed record MethodIsWaitingToBeGenerated
    {
        private MethodIsWaitingToBeGenerated()
        {
        }
    }
}

/// <summary>
/// 本库中所有的服务提供者都实现了 <see cref="IInitializingServiceProvider"/> 接口。<br/>
/// 但考虑到有可能业务使用了来自其他库的服务提供者，这时我们需要代理一下。
/// </summary>
internal sealed class InitializingServiceProviderProxy(IServiceProvider serviceProvider) : IInitializingServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return serviceProvider.GetService(serviceType);
    }
}
