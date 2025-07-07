using Avalonia;
using DotNetCampus.Terminal.Framework.DependencyInjection;

namespace DotNetCampus.Terminal.Framework;

public static class FrameworkExtensions
{
    /// <summary>
    /// 初始化依赖注入容器 <see cref="Container"/>。
    /// 在此设置之后，<see cref="Container.Current"/> 将可用。
    /// </summary>
    /// <param name="builder">参与应用程序启动流程中的应用创建器。</param>
    /// <param name="serviceCollectionBuilder">请在此方法中添加你要添加的服务。</param>
    /// <param name="options">
    /// <para>服务提供程序的选项。</para>
    /// 注意：<br/>
    ///  * 由于使用此方法初始化的是供应用程序全局使用的容器，所以这里默认提供的 <paramref name="options"/> 参数和类型 <see cref="ServiceProviderOptions"/> 构造函数创建出来的默认值是不同的。<br/>
    ///  * 此方法创建的服务提供者（<see cref="IServiceProvider"/>）默认是共享单例的，即 <see cref="ServiceProviderOptions.ShareSingletonInstances"/> 默认为 <see langword="true"/>；
    /// 而 <see cref="ServiceProviderOptions"/> 构造函数创建出来的默认值是 <see langword="false"/>。<br/>
    ///  * 所有共享单例的服务提供者所提供的单例服务是同一个实例。
    /// 所以如果你希望另一个服务提供者使用此全局的单例服务实例，那么请在创建另一个服务提供者时指定 <see cref="ServiceProviderOptions.ShareSingletonInstances"/> 为 <see langword="true"/>。
    /// </param>
    /// <returns>参与应用程序启动流程中的应用创建器。</returns>
    /// <remarks>
    /// 在应用初始化时可放心使用此方法收集整个应用程序的全部服务，因为服务的初始化是分步的，不会导致一开始加载完所有的程序集和类型对象。
    /// </remarks>
    public static AppBuilder UseContainer(this AppBuilder builder,
        Action<ServiceCollection> serviceCollectionBuilder, ServiceProviderOptions? options = null)
    {
        Container.Builder
            .Initialize(serviceCollectionBuilder)
            .InitializeToCurrentContainer(options ?? new ServiceProviderOptions
            {
                ShareSingletonInstances = true,
            });
        return builder;
    }
}
