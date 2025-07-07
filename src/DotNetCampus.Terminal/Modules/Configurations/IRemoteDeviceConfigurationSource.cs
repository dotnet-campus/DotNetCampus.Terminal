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
}
