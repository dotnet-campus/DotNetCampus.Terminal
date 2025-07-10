using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.Modules.Configurations.JsonSource;

/// <summary>
/// 设备配置根对象
/// </summary>
public record DeviceConfiguration
{
    /// <summary>
    /// SSH设备配置列表
    /// </summary>
    public List<SshRemoteDeviceInfo> SshDevices { get; set; } = [];
}
