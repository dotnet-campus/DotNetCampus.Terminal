using DotNetCampus.Terminal.Modules.Configurations.DebugSource;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Modules.Configurations.TomlSource;

namespace DotNetCampus.Terminal.Modules.Configurations;

public class ConfigurationManager
{
    private readonly List<IRemoteDeviceConfigurationSource> _remoteDeviceSources =
    [
        new DebugRemoteDeviceConfigurationSource(),
        new TomlRemoteDeviceConfigurationSource(),
    ];

    public async Task<IReadOnlyList<RemoteDeviceGroup>> FetchRemoteDevicesAsync()
    {
        var list = new List<RemoteDeviceGroup>();
        foreach (var source in _remoteDeviceSources)
        {
            var devices = await source.FetchRemoteDevicesAsync();
            list.Add(new RemoteDeviceGroup
            {
                Name = source.GroupName,
                Devices = devices,
            });
        }
        return list;
    }
}

public record RemoteDeviceGroup
{
    public required string Name { get; init; }

    public required IReadOnlyList<IRemoteDeviceInfo> Devices { get; init; } = [];
}
