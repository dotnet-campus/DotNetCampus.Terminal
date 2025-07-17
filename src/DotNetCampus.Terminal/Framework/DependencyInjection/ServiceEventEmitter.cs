namespace DotNetCampus.Terminal.Framework.DependencyInjection;

internal class ServiceEventEmitter(ServiceProvider serviceProvider, IReadOnlyList<PriorityServiceCollection> serviceCollections) : IServiceEventEmitter
{
    public void Emit(string eventId)
    {
        foreach (var serviceCollection in serviceCollections.Where(x => x.EventId == eventId))
        {
            foreach (var (type, _) in serviceCollection.GetCollectedServiceDescriptors())
            {
                // 通过获取服务来触发服务的初始化逻辑。
                _ = serviceProvider.GetService(type);
            }
        }
    }
}

/// <summary>
/// 如果注册服务时，有些服务与事件关联；那么此类型将引发这些事件，以便让这些服务立即初始化。
/// </summary>
public interface IServiceEventEmitter
{
    /// <summary>
    /// 引发指定名称的事件。
    /// </summary>
    /// <param name="eventId">事件名称。</param>
    /// <remarks>
    /// 重复引发相同名称的事件时，后续的事件将毫无效果。
    /// </remarks>
    void Emit(string eventId);
}
