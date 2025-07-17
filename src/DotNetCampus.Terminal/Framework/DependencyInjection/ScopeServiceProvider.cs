using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 一个从 <see cref="ServiceCollection"/> 中提供服务实例的带有作用域的服务提供者。<br/>
/// 同时也是带有作用域的服务提供者工厂。
/// </summary>
/// <param name="rootProvider">根级服务提供者，用于获取单例服务。</param>
/// <param name="serviceCollection">此服务作用域所提供的服务的来源集合。</param>
internal sealed class ScopeServiceProvider(
    ServiceProvider rootProvider,
    ServiceCollection serviceCollection)
    : IInitializingServiceProvider, IServiceScopeFactory, IServiceScope, IServiceProvider
{
    /// <summary>
    /// 0 = 正常工作。<br/>
    /// 1 = 正在处置（如果发生并发处置，则第一个会执行处置，其他会假设已完成处置）。<br/>
    /// 2 = 已处置完成（此刻开始，此服务提供者将不再提供服务）。
    /// </summary>
    private volatile int _disposedState;

    private readonly ConcurrentDictionary<Type, Lazy<object>> _scopedServiceInstances = [];
    private readonly ConcurrentBag<WeakReference<object>> _scopedTransientInstances = [];

    public IServiceProvider ServiceProvider => this;

    public object? GetService(Type serviceType)
    {
        ObjectDisposedException.ThrowIf(_disposedState is 2, this);

        var descriptor = serviceCollection.TryGetServiceDescriptor(serviceType);

        if (descriptor == null)
        {
            return null;
        }

        switch (descriptor.Lifetime)
        {
            // 单例服务。
            case ServiceLifetime.Singleton:
            {
                var instance = rootProvider.GetService(descriptor.ServiceType);
                // 不是自己创建的服务，不应 MapInstance。
                return instance;
            }
            // 作用域服务。
            case ServiceLifetime.Scoped:
            {
                var instance = _scopedServiceInstances.GetOrAdd(
                    serviceType,
                    _ => new Lazy<object>(() => descriptor.ImplementationFactory(this, descriptor.ServiceKey))).Value;
                this.MapInstance(instance);
                return instance;
            }
            // 瞬时服务。
            case ServiceLifetime.Transient:
            {
                var instance = descriptor.ImplementationFactory(this, descriptor.ServiceKey);
                _scopedTransientInstances.Add(new WeakReference<object>(instance));
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

    public T CreateTransient<T>() where T : notnull, new()
    {
        var instance = new T();
        _scopedTransientInstances.Add(new WeakReference<object>(instance));
        this.MapInstance(instance);
        return instance;
    }

    public T CreateTransient<T>(Func<IInitializingServiceProvider, T> factory)
        where T : notnull
    {
        var instance = factory(this);
        _scopedTransientInstances.Add(new WeakReference<object>(instance));
        this.MapInstance(instance);
        return instance;
    }

    IServiceScope IServiceScopeFactory.CreateScope()
    {
        // 作为服务提供者工厂使用时，此类型无法被处置。
        // 因此，其永远可以正确创建出新的作用域。
        return new ScopeServiceProvider(rootProvider, serviceCollection);
    }

    public IDisposable AttachScopeToRoot()
    {
        return rootProvider.CombineScopeServiceProvider(this);
    }

    public void Dispose() => Dispose(false);

    public void DisposeWithReferenceCheck() => Dispose(true);

    private void Dispose(bool checkReferences)
    {
        // 检查是否已经处置过。
        if (Interlocked.CompareExchange(ref _disposedState, 1, 0) is not 0)
        {
            // 只有首个调用 Dispose 的线程会执行处置操作。
            return;
        }

        // 释放作用域内的服务实例。
        DisposeScopeServices();

        // 记录作用域内的弱引用以备后续检查。
        List<WeakReference<object>>? stillExistReferences = null;
        if (checkReferences)
        {
            stillExistReferences = CollectStillExistReferences();
        }

        // 标记为已处置完成，此后不再提供服务。
        Interlocked.Exchange(ref _disposedState, 2);

        // 清空作用域内的服务实例。
        _scopedServiceInstances.Clear();
        _scopedTransientInstances.Clear();

        // 立即 GC 以检查是否有引用仍然存在。
        if (stillExistReferences is not null)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stillExistList = SelectStillExistReferenceNames(stillExistReferences);
            if (stillExistList.Count is 0)
            {
                // 非常好，所有作用域内的服务实例都已被正确释放。
            }
            else if (stillExistList.Count is 1)
            {
                throw new ScopeServiceReferenceNotGarbageCollectedException(stillExistList,
                    $"存在一个更长生命周期的实例引用了作用域内的服务：{stillExistList[0]}");
            }
            else
            {
                throw new ScopeServiceReferenceNotGarbageCollectedException(stillExistList,
                    $"存在更长生命周期的实例引用了 {stillExistList.Count} 个作用域内的服务：{string.Join(", ", stillExistList)}");
            }
        }
    }

    /// <summary>
    /// 单独使用一个不会内联的方法来释放作用域内的服务实例。
    /// </summary>
    /// <remarks>
    /// 要求不内联，是因为 lazyInstance 一旦被内联，会导致 GC 不回收这些实例。
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void DisposeScopeServices()
    {
        foreach (var lazyInstance in _scopedServiceInstances.Values)
        {
            if (lazyInstance is { IsValueCreated: true, Value: IDisposable disposable })
            {
                disposable.Dispose();
            }
        }
        foreach (var transientInstance in _scopedTransientInstances)
        {
            if (transientInstance.TryGetTarget(out var target) && target is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// 单独使用一个不会内联的方法来收集仍然存在的弱引用。
    /// </summary>
    /// <returns>仍然存在的弱引用列表。</returns>
    /// <remarks>
    /// 要求不内联，是因为 lazyInstance 一旦被内联，会导致 GC 不回收这些实例。
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private List<WeakReference<object>>? CollectStillExistReferences()
    {
        List<WeakReference<object>>? stillExistReferences = null;
        foreach (var lazyInstance in _scopedServiceInstances.Values)
        {
            if (lazyInstance.IsValueCreated)
            {
                stillExistReferences ??= [];
                stillExistReferences.Add(new WeakReference<object>(lazyInstance.Value));
            }
        }
        foreach (var transientInstance in _scopedTransientInstances)
        {
            stillExistReferences ??= [];
            stillExistReferences.Add(transientInstance);
        }
        return stillExistReferences;
    }

    /// <summary>
    /// 取出仍然存在的弱引用的类型名称列表。
    /// </summary>
    /// <param name="stillExistReferences">仍然存在的弱引用列表。</param>
    /// <returns>仍然存在的弱引用的类型名称列表。</returns>
    /// <remarks>
    /// 要求不内联，是因为后续内存分析时，不希望这些 Lambda 中捕获的实例被内存分析工具误认为是仍然存在的引用。
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static List<string> SelectStillExistReferenceNames(IReadOnlyList<WeakReference<object>> stillExistReferences)
    {
        return stillExistReferences
            .Select(reference => reference.TryGetTarget(out var target) ? target : null)
            .OfType<object>()
            .Select(x => x.GetType().Name)
            .ToList();
    }
}

/// <summary>
/// 用于创建 <see cref="IServiceScope"/> 实例的工厂，该实例用于在作用域内创建服务。
/// </summary>
public interface IServiceScopeFactory
{
    /// <summary>
    /// 创建一个 <see cref="IServiceScope"/>，
    /// 其中包含一个 <see cref="IServiceProvider"/>，用于从新创建的作用域中解析依赖项。
    /// </summary>
    /// <returns>
    /// 一个控制作用域生命周期的 <see cref="IServiceScope"/>。一旦该对象被释放，
    /// 所有从 <see cref="IServiceScope.ServiceProvider"/> 解析的作用域服务
    /// 也将被释放。
    /// </returns>
    IServiceScope CreateScope();
}

/// <summary>
/// <see cref="IDisposable.Dispose"/> 方法结束作用域的生命周期。一旦调用 Dispose，
/// 所有从 <see cref="IServiceScope.ServiceProvider"/> 解析的作用域服务也将被释放。
/// </summary>
public interface IServiceScope : IServiceProvider, IDisposable
{
    /// <summary>
    /// 用于从作用域中解析依赖项的 <see cref="System.IServiceProvider"/>。
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// 在没有提前声明瞬时服务的情况下，立即创建一个瞬时服务实例，并注入依赖。<br/>
    /// 通过此方法创建出来的实例，可以通过 <see cref="Container"/>.<see cref="Container.FromInstance"/> 获得服务提供者，
    /// 随后即可使用此服务提供者解析依赖的其他服务。
    /// </summary>
    /// <typeparam name="T">要创建的瞬时服务类型。</typeparam>
    /// <returns>创建好的瞬时服务实例。</returns>
    /// <remarks>
    /// 在作用域被回收后，此服务实例也会被回收。<br/>
    /// 如果实例实现了 <see cref="IDisposable"/> 接口，在回收时会自动调用其 <see cref="IDisposable.Dispose"/> 方法。
    /// </remarks>
    T CreateTransient<T>() where T : notnull, new();

    /// <summary>
    /// 在没有提前声明瞬时服务的情况下，立即创建一个瞬时服务实例，并注入依赖。<br/>
    /// 通过此方法创建出来的实例，可以通过 <see cref="Container"/>.<see cref="Container.FromInstance"/> 获得服务提供者，
    /// 随后即可使用此服务提供者解析依赖的其他服务。
    /// </summary>
    /// <param name="factory">创建瞬时服务实例的工厂方法。</param>
    /// <typeparam name="T">要创建的瞬时服务类型。</typeparam>
    /// <returns>创建好的瞬时服务实例。</returns>
    /// <remarks>
    /// 在作用域被回收后，此服务实例也会被回收。<br/>
    /// 如果实例实现了 <see cref="IDisposable"/> 接口，在回收时会自动调用其 <see cref="IDisposable.Dispose"/> 方法。
    /// </remarks>
    T CreateTransient<T>(Func<IInitializingServiceProvider, T> factory)
        where T : notnull;

    /// <summary>
    /// 临时将当前作用域的服务解析能力挂接到根服务提供者，使其在当前线程上下文及其异步流转中可见。
    /// </summary>
    /// <returns>
    /// 一个 <see cref="IDisposable"/>。在其被释放前，根服务提供者可解析到此作用域的服务；释放后则不可。
    /// </returns>
    /// <remarks>
    /// 此方法是线程安全的。
    /// <para>
    /// 仅在调用线程及其通过异步流转（如 <c>await</c>、<c>Task</c>、线程池等）传递的上下文中，
    /// 根服务提供者（如 <see cref="Container"/>.<see cref="Container.Current"/>）才能解析到当前作用域的服务实例。
    /// 与该上下文无关的其他线程无法访问此作用域服务；
    /// 甚至即便是同一线程，如果来自两个无关的上下文切换（例如两个独立的 Dispatcher.Invoke 调用），也无法访问此作用域服务。
    /// </para>
    /// <para>
    /// 典型用法如下：
    /// <code>
    /// using (scope.AttachScopeToRoot())
    /// {
    ///     // 在此期间，根服务提供者可解析到当前作用域的服务
    ///     var foo = Container.Current.EnsureGet&lt;FooService&gt;();
    /// }
    /// // 作用域挂接释放后，根服务提供者将无法再解析到该作用域的服务
    /// // 以下代码将抛出 InvalidOperationException
    /// var bar = Container.Current.EnsureGet&lt;FooService&gt;();
    /// </code>
    /// </para>
    /// <para>
    /// 支持多线程并发，每个线程上下文独立维护作用域挂接关系。
    /// 即使出现跨线程异步流转，也能正确获取到作用域服务实例。
    /// </para>
    /// </remarks>
    IDisposable AttachScopeToRoot();

    /// <summary>
    /// 内部会调用 <see cref="IDisposable.Dispose"/> 方法来结束作用域的生命周期。<br/>
    /// 但除此之外，此方法还会在释放服务之后，检查是否有引用仍然存在；<br/>
    /// 如果仍然存在引用，说明存在更长生命周期的实例引用了作用域内的服务，而这很容易造成不可预知的风险，所以我们会抛出异常。
    /// </summary>
    /// <remarks>
    /// 你可以使用如下用法，在 Debug 下检查引用，在 Release 下用内存泄露替代异常。
    /// <code>
    /// #if DEBUG
    /// scope.DisposeWithReferenceCheck();
    /// #else
    /// scope.Dispose();
    /// #endif
    /// </code>
    /// </remarks>
    void DisposeWithReferenceCheck();
}
