using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.Modules.Configurations.TomlSource;

public class TomlRemoteDeviceConfigurationSource : IRemoteDeviceConfigurationSource
{
    private readonly string _configurationPath;

    public TomlRemoteDeviceConfigurationSource()
    {
        _configurationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "terminal.toml");
    }

    public void Reload()
    {
        if (!File.Exists(_configurationPath))
        {
            return;
        }

    }

    public IReadOnlyList<IRemoteDeviceInfo> FetchRemoteDevicesAsync()
    {
        if (!File.Exists(_configurationPath))
        {
            return [];
        }

        return [];
    }
}
