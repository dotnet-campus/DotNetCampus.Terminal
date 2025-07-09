using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.ViewModels;

namespace DotNetCampus.Terminal.Utils;

/// <summary>
/// 提供对远程设备节点的扩展方法
/// </summary>
public static class RemoteDeviceNodeExtensions
{
    /// <summary>
    /// 获取节点是否有未保存的更改
    /// </summary>
    /// <param name="node">远程设备节点</param>
    /// <returns>如果有未保存的更改则返回true，否则返回false</returns>
    public static bool GetHasUnsavedChanges(this IRemoteDeviceNode node)
    {
        return node switch
        {
            TrackableBindableRecord trackable => trackable.HasUnsavedChanges,
            _ => false
        };
    }
    
    /// <summary>
    /// 重置节点的变更跟踪状态
    /// </summary>
    /// <param name="node">远程设备节点</param>
    public static void ResetChangeTracking(this IRemoteDeviceNode node)
    {
        if (node is TrackableBindableRecord trackable)
        {
            trackable.ResetChangeTracking();
        }
    }
}
