using System.Collections.Concurrent;

namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 为依赖注入收集和提供服务描述符的集合。
/// </summary>
public class ServiceCollection : IServiceCollection
{
    /// <summary>
    /// 如果此服务组合了其他服务集合，那么这里会记录这些服务集合。
    /// </summary>
    /// <remarks>
    /// 1. 会优先从整合的服务集合中获取服务。
    /// 2. 在整合的服务里，如果从当前服务集合中获取实例，很可能会获取到与被整合的服务相同的实例。
    /// </remarks>
    private readonly List<ServiceCollection> _ownedServiceCollections;

    /// <summary>
    /// 当前服务所收集的所有的带有优先级的服务集合。
    /// </summary>
    private readonly List<PriorityServiceCollection> _serviceCollections = [new PriorityServiceCollection()];

    /// <summary>
    /// 当 <see cref="ServiceProvider"/> 要求共享单例时，此集合会记录所有共享的单例实例。
    /// </summary>
    private readonly ConcurrentDictionary<Type, Lazy<object>> _sharedServiceInstances = new();

    /// <summary>
    /// 创建一个全新的服务集合。
    /// </summary>
    public ServiceCollection()
    {
        _ownedServiceCollections = [];
    }

    /// <summary>
    /// 基于已有的服务集合创建一个新的服务集合。
    /// </summary>
    /// <param name="ownedServiceCollections">已有的服务集合。</param>
    public ServiceCollection(IEnumerable<ServiceCollection> ownedServiceCollections)
    {
        _ownedServiceCollections = [..ownedServiceCollections];
    }

    /// <summary>
    /// 判断此服务集合是否为空。
    /// </summary>
    internal bool IsEmpty => _ownedServiceCollections.Count is 0 && _serviceCollections.Count is 1;

    IServiceCollection IServiceCollection.AddSingleton<T>(Func<IInitializingServiceProvider, T> implementationFactory)
        => AddSingleton(implementationFactory);

    IServiceCollection IServiceCollection.AddScoped<T>(Func<IInitializingServiceProvider, T> implementationFactory)
        => AddScoped(implementationFactory);

    IServiceCollection IServiceCollection.AddTransient<T>(Func<IInitializingServiceProvider, T> implementationFactory)
        => AddTransient(implementationFactory);

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的单例服务。
    /// </summary>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    /// <remarks>
    /// 请注意，虽然使用此方法添加的服务实例在被请求时才会初始化，但服务类型 <typeparamref name="T"/> 的类型对象会被立即初始化，
    /// 这可能会递归地初始化此类型对象所涉及的所有类型对象，导致意料之外的性能开销。<br/>
    /// 所以，建议你仅在应用程序每次启动都必定会用到的服务使用此方法初始化。如果不是，建议改用 <see cref="AddLazyServices"/> 方法以提升启动性能。
    /// </remarks>
    public ServiceCollection AddSingleton<T>(Func<IInitializingServiceProvider, T> implementationFactory) where T : notnull
    {
        _serviceCollections[0].AddSingleton(implementationFactory);
        return this;
    }

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的作用域服务。
    /// </summary>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    /// <remarks>
    /// 请注意，虽然使用此方法添加的服务实例在被请求时才会初始化，但服务类型 <typeparamref name="T"/> 的类型对象会被立即初始化，
    /// 这可能会递归地初始化此类型对象所涉及的所有类型对象，导致意料之外的性能开销。<br/>
    /// 所以，建议你仅在应用程序每次启动都必定会用到的服务使用此方法初始化。如果不是，建议改用 <see cref="AddLazyServices"/> 方法以提升启动性能。
    /// </remarks>
    public ServiceCollection AddScoped<T>(Func<IInitializingServiceProvider, T> implementationFactory) where T : notnull
    {
        _serviceCollections[0].AddScoped(implementationFactory);
        return this;
    }

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的瞬时服务。
    /// </summary>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    /// <remarks>
    /// 请注意，虽然使用此方法添加的服务实例在被请求时才会初始化，但服务类型 <typeparamref name="T"/> 的类型对象会被立即初始化，
    /// 这可能会递归地初始化此类型对象所涉及的所有类型对象，导致意料之外的性能开销。<br/>
    /// 所以，建议你仅在应用程序每次启动都必定会用到的服务使用此方法初始化。如果不是，建议改用 <see cref="ServiceCollection.AddLazyServices"/> 方法以提升启动性能。
    /// </remarks>
    public ServiceCollection AddTransient<T>(Func<IInitializingServiceProvider, T> implementationFactory) where T : notnull
    {
        _serviceCollections[0].AddTransient(implementationFactory);
        return this;
    }

    /// <summary>
    /// 向服务集合中延迟添加一组服务。
    /// </summary>
    /// <param name="serviceCollectionBuilder">使用此服务集合添加服务。</param>
    /// <returns>提供链式调用的服务集合。</returns>
    public ServiceCollection AddLazyServices(Action<PriorityServiceCollection> serviceCollectionBuilder)
    {
        var priorityCollection = new PriorityServiceCollection(null, serviceCollectionBuilder);
        _serviceCollections.Add(priorityCollection);
        return this;
    }

    /// <summary>
    /// 向服务集合中添加一组关联某个事件 <paramref name="eventId"/> 的服务。
    /// </summary>
    /// <param name="eventId">服务集合关联的事件 Id。</param>
    /// <param name="serviceCollectionBuilder">使用此服务集合添加服务。</param>
    /// <returns>提供链式调用的服务集合。</returns>
    /// <remarks>
    /// 这组集合内的服务会在以下情况之一发生时被初始化：
    /// <list type="bullet">
    /// <item>当事件 <paramref name="eventId"/> 发生时。</item>
    /// <item>当此服务的实例被请求时。</item>
    /// </list>
    /// </remarks>
    public ServiceCollection AddEventServices(string eventId, Action<PriorityServiceCollection> serviceCollectionBuilder)
    {
        var priorityCollection = new PriorityServiceCollection(eventId, serviceCollectionBuilder);
        _serviceCollections.Add(priorityCollection);
        return this;
    }

    /// <summary>
    /// 创建一个能为此服务集合提供服务的服务提供者。
    /// </summary>
    /// <returns>一个服务提供者。</returns>
    public IServiceProvider BuildServiceProvider(ServiceProviderOptions? options = null)
    {
        // 创建服务提供者实例。
        var serviceProvider = options?.ShareSingletonInstances is true
            ? new ServiceProvider(this, GetSharedSingleton)
            : new ServiceProvider(this);

        // 添加服务提供器本身机制正常工作所需的服务。
        _serviceCollections[0].TryAdd<IServiceScopeFactory>(new ServiceDescriptor(
            typeof(IServiceScopeFactory),
            _ => new ScopeServiceProvider(serviceProvider, this))
        {
            Lifetime = ServiceLifetime.Singleton,
        });
        _serviceCollections[0].TryAdd<IServiceEventEmitter>(new ServiceDescriptor(
            typeof(IServiceEventEmitter),
            _ => new ServiceEventEmitter(serviceProvider, _serviceCollections))
        {
            Lifetime = ServiceLifetime.Singleton,
        });

        // 返回服务提供者实例。
        return serviceProvider;
    }

    /// <summary>
    /// 尝试从服务集合中获取指定类型的服务描述符。
    /// </summary>
    /// <param name="serviceType">要获取的服务类型。</param>
    /// <returns>如果找到了指定类型的服务描述符，则返回该描述符；否则返回 <see langword="null"/>。</returns>
    internal ServiceDescriptor? TryGetServiceDescriptor(Type serviceType)
    {
        // 先遍历一级服务集合（也就是业务初始化时直接添加的那些服务）。
        for (var i = 0; i < _ownedServiceCollections.Count; i++)
        {
            var owned = _ownedServiceCollections[i];
            var descriptor = owned.TryGetServiceDescriptor(serviceType);

            if (descriptor != null)
            {
                return descriptor;
            }
        }

        // 再遍历延迟服务集合（也就是业务初始化时通过 AddLazyServices 添加的那些服务）。
        // 依次遍历，这样才能确保延迟服务在不需要获取的时候，连类型都不会初始化。
        for (var i = 0; i < _serviceCollections.Count; i++)
        {
            var collection = _serviceCollections[i];
            var descriptor = collection.TryGetServiceDescriptor(serviceType);

            if (descriptor != null)
            {
                return descriptor;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取共享单例服务实例。
    /// </summary>
    /// <param name="serviceProvider">服务提供者。</param>
    /// <param name="descriptor">服务描述符。</param>
    /// <returns>共享单例服务实例。</returns>
    private object GetSharedSingleton(ServiceProvider serviceProvider, ServiceDescriptor descriptor)
    {
        return _sharedServiceInstances.GetOrAdd(
                descriptor.ServiceType,
                _ => new Lazy<object>(() => descriptor.ImplementationFactory(serviceProvider, descriptor.ServiceKey)))
            .Value;
    }
}
