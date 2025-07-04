using System.Collections.Concurrent;

namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 一个从 <see cref="ServiceCollection"/> 中提供服务实例的服务提供者。
/// </summary>
/// <param name="serviceCollection">此服务提供者所提供的服务的来源集合。</param>
/// <param name="sharedSingletonGetter">当服务提供者要求共享单例时，此委托会被调用以获取共享的单例实例。</param>
internal class ServiceProvider(
    ServiceCollection serviceCollection,
    Func<ServiceProvider, ServiceDescriptor, object?>? sharedSingletonGetter = null)
    : IInitializingServiceProvider, IServiceProvider
{
    private readonly WeakAsyncLocalAccessor<IServiceProvider> _scopeServiceProvider = new();

    private readonly ConcurrentDictionary<Type, Lazy<object>> _serviceInstances = [];

    internal IDisposable CombineScopeServiceProvider(IServiceProvider serviceProvider)
    {
        _scopeServiceProvider.Value = serviceProvider;
        return new ActionDisposable(serviceProvider, () => _scopeServiceProvider.Value = null);
    }

    public object? GetService(Type serviceType)
    {
        var descriptor = serviceCollection.TryGetServiceDescriptor(serviceType);

        if (descriptor == null)
        {
            return null;
        }

        switch (descriptor.Lifetime)
        {
            // 单例服务。
            case ServiceLifetime.Singleton when sharedSingletonGetter is not null:
            {
                // 共享单例服务。
                var instance = sharedSingletonGetter(this, descriptor);
                this.MapInstance(instance);
                return instance;
            }
            case ServiceLifetime.Singleton:
            {
                // 独立单例服务。
                var instance = _serviceInstances.GetOrAdd(
                    serviceType,
                    _ => new Lazy<object>(() => descriptor.ImplementationFactory(this, descriptor.ServiceKey))).Value;
                this.MapInstance(instance);
                return instance;
            }
            // 作用域服务。
            case ServiceLifetime.Scoped:
            {
                if (_scopeServiceProvider.Value is { } scopeServiceProvider)
                {
                    var instance = scopeServiceProvider.GetService(descriptor.ServiceType);
                    // 不是自己创建的服务，不应 MapInstance。
                    return instance;
                }

                // 作用域服务已由 ServiceScopeFactory 实现。
                // 但使用全局的 ServiceProvider 直接获取作用域服务是不正确的使用方式。
                throw new InvalidOperationException(
                    $"不应该直接从全局的 {nameof(IServiceProvider)} 获取作用域服务。请使用 {nameof(ServiceExtensions.CreateScope)} 扩展方法创建作用域以获取此作用域服务。");
            }
            // 瞬时服务。
            case ServiceLifetime.Transient:
            {
                var instance = descriptor.ImplementationFactory(this, descriptor.ServiceKey);
                this.MapInstance(instance);
                return instance;
            }
            // 未知的生命周期。
            default:
            {
                return null;
            }
        }
    }
}

file sealed class ActionDisposable(object holdInstance, Action disposeAction) : IDisposable
{
    /// <summary>
    /// 我们需要保留此字段以便在本实例不被 GC 时，holdInstance 实例一定不会被 GC。
    /// </summary>
    private readonly object _holdInstance = holdInstance;

    ~ActionDisposable()
    {
        Dispose();
    }

    public void Dispose()
    {
        // 如果出现了并发多次执行，那就多次执行吧，不影响的。
        _ = _holdInstance;
        disposeAction();
        GC.SuppressFinalize(this);
    }
}
