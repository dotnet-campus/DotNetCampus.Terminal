using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.Modules.Configurations.DebugSource;

public class DebugRemoteDeviceConfigurationSource : IRemoteDeviceConfigurationSource
{
    public string GroupName => "仅供调试";

    public Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync()
    {
        return Task.FromResult<IReadOnlyList<IRemoteDeviceInfo>>(
        [
            new SshRemoteDeviceInfo
            {
                ConnectionName = "测试设备 1",
                HostName = "172.20.114.71",
                Port = "22",
                UserName = "seewo",
                Password = "123",
            },
            new SshRemoteDeviceInfo
            {
                ConnectionName = "测试设备 2",
                HostName = "172.20.114.71",
                Port = "22",
                UserName = "seewo",
                Password = "123",
            },
            new SshRemoteDeviceInfo
            {
                ConnectionName = "测试设备 3",
                HostName = "172.20.114.71",
                Port = "22",
                UserName = "seewo",
                Password = "123",
            },
        ]);
    }
}
