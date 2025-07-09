using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.Modules.Configurations;

/// <summary>
/// 远程设备配置源接口。
/// </summary>
public interface IRemoteDeviceConfigurationSource
{
    /// <summary>
    /// 配置组名称。
    /// </summary>
    string GroupName { get; }

    /// <summary>
    /// 获取远程设备信息列表。
    /// </summary>
    /// <returns>远程设备信息列表。</returns>
    Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync();

    /// <summary>
    /// 保存单个远程设备信息。
    /// </summary>
    /// <param name="deviceInfo">要保存的设备信息。</param>
    /// <returns>表示异步操作的任务。</returns>
    Task SaveRemoteDeviceAsync(IRemoteDeviceInfo deviceInfo);

    /// <summary>
    /// 删除指定的远程设备信息。
    /// </summary>
    /// <param name="connectionName">要删除的设备连接名称。</param>
    /// <returns>表示异步操作的任务。</returns>
    Task RemoveRemoteDeviceAsync(string connectionName);
}
