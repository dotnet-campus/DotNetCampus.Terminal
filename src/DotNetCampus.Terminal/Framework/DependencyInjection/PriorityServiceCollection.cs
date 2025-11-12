using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 为依赖注入收集和提供服务描述符的集合。
/// </summary>
public class PriorityServiceCollection : IServiceCollection
{
    private readonly ConcurrentDictionary<Type, ServiceDescriptor> _collectingServiceDescriptors = [];
    private readonly Lazy<FrozenDictionary<Type, ServiceDescriptor>> _collectedServiceDescriptors;

    internal PriorityServiceCollection()
    {
        _collectedServiceDescriptors = new Lazy<FrozenDictionary<Type, ServiceDescriptor>>(() =>
            _collectingServiceDescriptors.ToFrozenDictionary(ReferenceEqualityComparer.Instance));
    }

    internal PriorityServiceCollection(string? eventId, Action<PriorityServiceCollection> serviceCollectionBuilder)
    {
        EventId = eventId;
        _collectedServiceDescriptors = new Lazy<FrozenDictionary<Type, ServiceDescriptor>>(() =>
        {
            serviceCollectionBuilder(this);
            return _collectingServiceDescriptors.ToFrozenDictionary();
        });
    }

    /// <summary>
    /// 获取与这一组服务相关联的事件 Id。
    /// </summary>
    public string? EventId { get; }

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
    public PriorityServiceCollection AddSingleton<T>(Func<IInitializingServiceProvider, T> implementationFactory) where T : notnull
    {
        Add<T>(new ServiceDescriptor(typeof(T), s => implementationFactory(s))
        {
            Lifetime = ServiceLifetime.Singleton,
        });
        return this;
    }

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的作用域服务。
    /// </summary>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    public PriorityServiceCollection AddScoped<T>(Func<IInitializingServiceProvider, T> implementationFactory) where T : notnull
    {
        Add<T>(new ServiceDescriptor(typeof(T), s => implementationFactory(s))
        {
            Lifetime = ServiceLifetime.Scoped,
        });
        return this;
    }

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的瞬时服务。
    /// </summary>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    public PriorityServiceCollection AddTransient<T>(Func<IInitializingServiceProvider, T> implementationFactory) where T : notnull
    {
        Add<T>(new ServiceDescriptor(typeof(T), s => implementationFactory(s))
        {
            Lifetime = ServiceLifetime.Transient,
        });
        return this;
    }

    /// <summary>
    /// 向服务集合中添加一个服务类型为 <typeparamref name="T"/> 的服务描述符。
    /// </summary>
    /// <param name="serviceDescriptor">服务描述符。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <exception cref="InvalidOperationException">如果服务已经被添加过，则抛出此异常。</exception>
    private void Add<T>(ServiceDescriptor serviceDescriptor) where T : notnull
    {
        var added = _collectingServiceDescriptors.TryAdd(typeof(T), serviceDescriptor);

        if (!added)
        {
            throw new InvalidOperationException($"Service {typeof(T).FullName} has already been added.");
        }
    }

    /// <summary>
    /// 向服务集合中添加一个服务类型为 <typeparamref name="T"/> 的服务描述符。
    /// </summary>
    /// <param name="serviceDescriptor">服务描述符。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>
    /// 如果成功添加了服务描述符，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。<br/>
    /// <see langword="false"/> 意味着其他业务已经添加了相同类型的服务描述符。
    /// </returns>
    internal bool TryAdd<T>(ServiceDescriptor serviceDescriptor) where T : notnull
    {
        var added = _collectingServiceDescriptors.TryAdd(typeof(T), serviceDescriptor);
        return added;
    }

    /// <summary>
    /// 尝试从服务集合中获取指定类型的服务描述符。
    /// </summary>
    /// <param name="serviceType">要获取的服务类型。</param>
    /// <returns>如果找到了指定类型的服务描述符，则返回该描述符；否则返回 <see langword="null"/>。</returns>
    public ServiceDescriptor? TryGetServiceDescriptor(Type serviceType)
    {
        return _collectedServiceDescriptors.Value.TryGetValue(serviceType, out var value)
            ? value
            : null;
    }

    /// <summary>
    /// 获取已收集的服务描述符集合。
    /// </summary>
    /// <returns>已收集的服务描述符集合。</returns>
    internal FrozenDictionary<Type, ServiceDescriptor> GetCollectedServiceDescriptors()
    {
        return _collectedServiceDescriptors.Value;
    }
}
