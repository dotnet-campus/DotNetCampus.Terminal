using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.Modules.Configurations.TomlSource;

public class TomlRemoteDeviceConfigurationSource : IRemoteDeviceConfigurationSource
{
    private readonly string _configurationPath;

    public TomlRemoteDeviceConfigurationSource()
    {
        _configurationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "terminal.toml");
    }

    public string GroupName => "桌面配置文件";

    public async Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync()
    {
        await Task.Yield();

        if (!File.Exists(_configurationPath))
        {
            return [];
        }

        return [];
    }
}
