namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 为服务集合、服务提供者和容器提供扩展方法。
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// 从容器中获取指定类型的实例。
    /// </summary>
    /// <param name="provider">服务提供者。</param>
    /// <typeparam name="T">要获取的类型。</typeparam>
    /// <returns>返回获取到的实例。</returns>
    public static T EnsureGet<T>(this IServiceProvider provider) where T : notnull
    {
        var instance = provider.Get<T>();
        if (instance is null)
        {
            throw new InvalidOperationException($"Cannot find service of type {typeof(T)}.");
        }
        return instance;
    }

    /// <summary>
    /// 从容器中获取指定类型的实例。如果容器中没有指定类型的实例，则返回 <see langword="null"/>。
    /// 如果希望确保获取到实例，可以使用 <see cref="EnsureGet{T}(System.IServiceProvider)"/> 方法。
    /// </summary>
    /// <param name="provider">服务提供者。</param>
    /// <typeparam name="T">要获取的类型。</typeparam>
    /// <returns>返回获取到的实例，如果没有找到则返回 <see langword="null"/>。</returns>
    public static T? Get<T>(this IServiceProvider provider) where T : notnull
    {
        return (T?)provider.GetService(typeof(T));
    }

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的单例服务。
    /// </summary>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    /// <remarks>
    /// 请注意，虽然使用此方法添加的服务实例在被请求时才会初始化，但服务类型 <typeparamref name="T"/> 的类型对象会被立即初始化，
    /// 这可能会递归地初始化此类型对象所涉及的所有类型对象，导致意料之外的性能开销。<br/>
    /// 所以，建议你仅在应用程序每次启动都必定会用到的服务使用此方法初始化。如果不是，建议改用 <see cref="ServiceCollection.AddLazyServices"/> 方法以提升启动性能。
    /// </remarks>
    public static ServiceCollection AddSingleton<T>(this ServiceCollection collection)
        where T : notnull, new()
    {
        return collection.AddSingleton(_ => new T());
    }

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的作用域服务。
    /// </summary>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    /// <remarks>
    /// 请注意，虽然使用此方法添加的服务实例在被请求时才会初始化，但服务类型 <typeparamref name="T"/> 的类型对象会被立即初始化，
    /// 这可能会递归地初始化此类型对象所涉及的所有类型对象，导致意料之外的性能开销。<br/>
    /// 所以，建议你仅在应用程序每次启动都必定会用到的服务使用此方法初始化。如果不是，建议改用 <see cref="ServiceCollection.AddLazyServices"/> 方法以提升启动性能。
    /// </remarks>
    public static ServiceCollection AddScoped<T>(this ServiceCollection collection)
        where T : notnull, new()
    {
        return collection.AddScoped(_ => new T());
    }

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的瞬时服务。
    /// </summary>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    /// <remarks>
    /// 请注意，虽然使用此方法添加的服务实例在被请求时才会初始化，但服务类型 <typeparamref name="T"/> 的类型对象会被立即初始化，
    /// 这可能会递归地初始化此类型对象所涉及的所有类型对象，导致意料之外的性能开销。<br/>
    /// 所以，建议你仅在应用程序每次启动都必定会用到的服务使用此方法初始化。如果不是，建议改用 <see cref="ServiceCollection.AddLazyServices"/> 方法以提升启动性能。
    /// </remarks>
    public static ServiceCollection AddTransient<T>(this ServiceCollection collection)
        where T : notnull, new()
    {
        return collection.AddTransient(_ => new T());
    }

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的单例服务。
    /// </summary>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    public static PriorityServiceCollection AddSingleton<T>(this PriorityServiceCollection collection)
        where T : notnull, new()
    {
        return collection.AddSingleton(_ => new T());
    }

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的瞬时服务。
    /// </summary>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    public static PriorityServiceCollection AddTransient<T>(this PriorityServiceCollection collection)
        where T : notnull, new()
    {
        return collection.AddTransient(_ => new T());
    }

    /// <summary>
    /// 创建一个可用于解析作用域服务的新 <see cref="IServiceScope"/>。
    /// </summary>
    /// <param name="provider">用于创建作用域的 <see cref="IServiceProvider"/>。</param>
    /// <returns>一个可用于解析作用域服务的 <see cref="IServiceScope"/>。</returns>
    public static IServiceScope CreateScope(this IServiceProvider provider)
    {
        return provider.EnsureGet<IServiceScopeFactory>().CreateScope();
    }

    /// <summary>
    /// 在没有提前声明瞬时服务的情况下，立即创建一个瞬时服务实例，并注入依赖。<br/>
    /// 通过此方法创建出来的实例，可以通过 <see cref="Container"/>.<see cref="Container.FromInstance"/> 获得服务提供者，
    /// 随后即可使用此服务提供者解析依赖的其他服务。
    /// </summary>
    /// <param name="provider">使用此服务提供者解析依赖。</param>
    /// <typeparam name="T">要创建的瞬时服务类型。</typeparam>
    /// <returns>创建好的瞬时服务实例。</returns>
    public static T CreateTransient<T>(this IServiceProvider provider)
        where T : notnull, new()
    {
        if (provider is IServiceScope serviceScope)
        {
            return serviceScope.CreateTransient<T>();
        }

        var instance = new T();
        provider.MapInstance(instance);
        return instance;
    }

    /// <summary>
    /// 在没有提前声明瞬时服务的情况下，立即创建一个瞬时服务实例，并注入依赖。<br/>
    /// 通过此方法创建出来的实例，可以通过 <see cref="Container"/>.<see cref="Container.FromInstance"/> 获得服务提供者，
    /// 随后即可使用此服务提供者解析依赖的其他服务。
    /// </summary>
    /// <param name="provider">使用此服务提供者解析依赖。</param>
    /// <param name="factory">创建瞬时服务实例的工厂方法。</param>
    /// <typeparam name="T">要创建的瞬时服务类型。</typeparam>
    /// <returns>创建好的瞬时服务实例。</returns>
    public static T CreateTransient<T>(this IServiceProvider provider, Func<IInitializingServiceProvider, T> factory)
        where T : notnull
    {
        if (provider is IServiceScope serviceScope)
        {
            return serviceScope.CreateTransient(factory);
        }

        var initializingProvider = IInitializingServiceProvider.From(provider);
        var instance = factory(initializingProvider);
        provider.MapInstance(instance);
        return instance;
    }

    /// <summary>
    /// 如果某些服务在注册时与指定名称的事件关联，那么当引发此名称的事件时，这些服务会立即初始化。<br/>
    /// 通过此方法，你可以获得一个专属于此 <see cref="IServiceProvider"/> 的服务事件发射器，用于初始化这些服务。
    /// </summary>
    /// <param name="provider">与服务事件发射器关联的 <see cref="IServiceProvider"/>。</param>
    /// <returns>服务事件发射器实例。</returns>
    public static IServiceEventEmitter GetServiceEventEmitter(this IServiceProvider provider)
    {
        return provider.EnsureGet<IServiceEventEmitter>();
    }
}
