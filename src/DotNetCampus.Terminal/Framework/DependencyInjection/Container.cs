using System.Runtime.CompilerServices;

namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 提供依赖注入服务的静态类。
/// </summary>
public static class Container
{
    /// <summary>
    /// 设置全局依赖注入容器时，需要保证线程安全。
    /// </summary>
    private static readonly object Locker = new();

    /// <summary>
    /// 映射所有从 <see cref="IServiceProvider"/> 中创建的服务实例，使其可以轻易找到创建它的 <see cref="IServiceProvider"/>。
    /// </summary>
    private static readonly ConditionalWeakTable<object, IServiceProvider> InstanceToServiceProviderCache = [];

    /// <summary>
    /// 获取或设置全局依赖注入容器的实例。
    /// </summary>
    /// <remarks>
    /// 注意，通过此属性只能获取到全局注入的服务。如果需要获取局部模块或机制特有的服务，请想办法获取到对应的容器实例。
    /// </remarks>
    public static IServiceProvider Current { get; private set; } = new UninitializedServiceProvider();

    /// <summary>
    /// 获取创建了当前服务实例的服务提供者。利用此服务提供者，你可以获取到同组的其他服务实例。
    /// </summary>
    /// <param name="instance">用来获取服务提供者的实例。</param>
    /// <returns>同组的服务提供者。</returns>
    /// <exception cref="InvalidOperationException">如果实例不是通过 <see cref="IServiceProvider"/> 创建的，则抛出此异常。</exception>
    public static IServiceProvider FromInstance(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance, nameof(instance));

        if (instance is IServiceProvider serviceProvider)
        {
            return serviceProvider;
        }

        if (instance is IServiceProviderAware service)
        {
            return service.ServiceProvider ?? throw new InvalidOperationException(
                $"在实现 {nameof(IServiceProviderAware)} 接口的服务时，必须在构造函数中通过 {nameof(IServiceProvider)} 参数初始化 {nameof(IServiceProviderAware.ServiceProvider)} 属性。");
        }

        if (InstanceToServiceProviderCache.TryGetValue(instance, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"""
            此实例（{instance.GetType().FullName}）并不是通过 IServiceProvider 创建的，因此无法获取到其同组的服务提供者。
            要解决此问题，你可以尝试以下方法之一：
            1. 确保此类型的实例是通过 IServiceProvider.GetService 之类的方式创建的。
            2. 找到你的代码中能够获取到的从 IServiceProvider 创建的其他实例，并使用它们的 GetServiceProvider() 方法获取到同组的服务提供者。
            """);
    }

    /// <summary>
    /// 将一个实例与其创建的服务提供者关联起来。
    /// </summary>
    /// <param name="serviceProvider">要关联的服务提供者。</param>
    /// <param name="instance">要关联的实例。</param>
    internal static void MapInstance(this IServiceProvider serviceProvider, object? instance)
    {
        if (instance is null)
        {
            return;
        }

        if (instance is IServiceProviderAware)
        {
            // 如果实例已经实现了 IServiceProviderAware 接口，由于后续可直接从属性获取，所以无需额外添加映射关系。
            return;
        }

        InstanceToServiceProviderCache.AddOrUpdate(instance, serviceProvider);
    }

    /// <summary>
    /// 为了确保业务在从容器获取服务之前一定完成了初始化，这里提供一个未初始化的服务通过异常提示开发者。
    /// </summary>
    private sealed class UninitializedServiceProvider : IServiceProvider
    {
        object? IServiceProvider.GetService(Type serviceType)
        {
            throw new InvalidOperationException($"在取得任何服务之前，必须先调用 {nameof(FrameworkExtensions)}.{nameof(FrameworkExtensions.UseContainer)} 方法初始化容器。");
        }
    }

    /// <summary>
    /// 提供构造容器的方法。
    /// </summary>
    internal static class Builder
    {
        /// <summary>
        /// 初始化一份容器构造器，随后你可以使用此方法的返回值以初始化其他容器。
        /// </summary>
        /// <param name="initializer">请在此方法中添加你的服务。</param>
        /// <returns>一个已初始化了服务的容器构造器。</returns>
        public static InitializedContainerBuilder Initialize(Action<ServiceCollection> initializer)
        {
            var collection = new ServiceCollection();
            initializer(collection);
            return new InitializedContainerBuilder(collection);
        }

        /// <summary>
        /// 表示一个已初始化了服务的容器构造器。
        /// </summary>
        /// <param name="collection">已初始化了服务的容器。</param>
        public sealed class InitializedContainerBuilder(ServiceCollection collection)
        {
            /// <summary>
            /// 将此已初始化了服务的容器构造器初始化到全局容器中。请注意，全局容器只允许被初始化一次。
            /// </summary>
            /// <param name="options">服务提供程序的选项。</param>
            /// <exception cref="InvalidOperationException">
            /// 全局容器只允许被初始化一次。如果被多次初始化，将会抛出此异常。
            /// </exception>
            public void InitializeToCurrentContainer(ServiceProviderOptions? options = null)
            {
                if (Current is not UninitializedServiceProvider)
                {
                    throw new InvalidOperationException("全局容器只允许被初始化一次。如果希望在容器中添加更多服务，请使用 CreateChildContainer 方法创建子容器。");
                }

                Current = collection.BuildServiceProvider(options);
            }
        }
    }
}
