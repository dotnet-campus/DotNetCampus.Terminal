namespace DotNetCampus.Terminal.Framework.DependencyInjection;

/// <summary>
/// 为依赖注入收集和提供服务描述符的集合。
/// </summary>
public interface IServiceCollection
{
    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的单例服务。
    /// </summary>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    IServiceCollection AddSingleton<T>(Func<IInitializingServiceProvider, T> implementationFactory) where T : notnull;

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的作用域服务。
    /// </summary>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    IServiceCollection AddScoped<T>(Func<IInitializingServiceProvider, T> implementationFactory) where T : notnull;

    /// <summary>
    /// 向服务集合中添加一个类型为 <typeparamref name="T"/> 的瞬时服务。
    /// </summary>
    /// <param name="implementationFactory">服务的实现工厂。</param>
    /// <typeparam name="T">服务的类型。</typeparam>
    /// <returns>提供链式调用的服务集合。</returns>
    IServiceCollection AddTransient<T>(Func<IInitializingServiceProvider, T> implementationFactory) where T : notnull;
}
