using DotNetCampus.Terminal.Modules.Configurations.Models;
using Tomlet;

namespace DotNetCampus.Terminal.Modules.Configurations.TomlSource;

/// <summary>
/// 基于 TOML 文件的远程设备配置源
/// </summary>
public class TomlRemoteDeviceConfigurationSource : IRemoteDeviceConfigurationSource
{
    private readonly string _configurationPath;

    public TomlRemoteDeviceConfigurationSource()
    {
        _configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "terminal.toml");
    }

    /// <summary>
    /// 使用指定的配置文件路径
    /// </summary>
    /// <param name="configurationPath">配置文件路径</param>
    public TomlRemoteDeviceConfigurationSource(string configurationPath)
    {
        _configurationPath = configurationPath;
    }

    public string GroupName => "TOML 配置文件";

    public async Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync()
    {
        try
        {
            if (!File.Exists(_configurationPath))
            {
                return [];
            }

            var tomlContent = await File.ReadAllTextAsync(_configurationPath);
            var deviceConfiguration = TomletMain.To<TomlDeviceConfiguration>(tomlContent);

            var devices = new List<IRemoteDeviceInfo>();

            foreach (var sshDevice in deviceConfiguration.SshDevices)
            {
                var deviceInfo = new SshRemoteDeviceInfo
                {
                    ConnectionName = sshDevice.ConnectionName,
                    Host = sshDevice.Host,
                    Port = sshDevice.Port,
                    UserName = sshDevice.UserName,
                    Password = sshDevice.Password,
                    SyncGroups = sshDevice.SyncGroups,
                };

                devices.Add(deviceInfo);
            }

            return devices;
        }
        catch (Exception ex)
        {
            // 记录错误但不中断程序
            // TODO: 添加日志记录
            Console.WriteLine($"加载 TOML 配置文件失败: {ex.Message}");
            return [];
        }
    }
}
