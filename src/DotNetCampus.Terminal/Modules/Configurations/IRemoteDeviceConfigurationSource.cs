using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.Modules.Configurations;

public interface IRemoteDeviceConfigurationSource
{
    IReadOnlyList<IRemoteDeviceInfo> FetchRemoteDevicesAsync();
}
