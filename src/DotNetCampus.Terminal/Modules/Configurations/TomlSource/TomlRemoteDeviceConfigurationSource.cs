using DotNetCampus.Terminal.Modules.Configurations.Models;
using Tomlet;
using DotNetCampus.Logging;

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

    public async Task SaveRemoteDeviceAsync(IRemoteDeviceInfo deviceInfo)
    {
        try
        {
            if (deviceInfo is not SshRemoteDeviceInfo sshDeviceInfo)
            {
                Log.Error($"[Config] 不支持的设备类型: {deviceInfo.GetType().Name}");
                return;
            }

            Log.Info($"[Config] 开始保存设备配置: {sshDeviceInfo.ConnectionName}");

            // 读取现有配置
            var deviceConfiguration = await LoadTomlConfigurationAsync();

            // 查找是否已存在同名设备
            var existingDeviceIndex = deviceConfiguration.SshDevices.FindIndex(d => 
                d.ConnectionName.Equals(sshDeviceInfo.ConnectionName, StringComparison.OrdinalIgnoreCase));

            // 转换为 TOML 设备配置
            var tomlDevice = ConvertToTomlDevice(sshDeviceInfo);

            if (existingDeviceIndex >= 0)
            {
                // 更新现有设备
                deviceConfiguration.SshDevices[existingDeviceIndex] = tomlDevice;
                Log.Info($"[Config] 更新现有设备配置: {sshDeviceInfo.ConnectionName}");
            }
            else
            {
                // 添加新设备
                deviceConfiguration.SshDevices.Add(tomlDevice);
                Log.Info($"[Config] 添加新设备配置: {sshDeviceInfo.ConnectionName}");
            }

            // 保存到文件
            await SaveTomlConfigurationAsync(deviceConfiguration);

            Log.Info($"[Config] 设备配置保存成功: {sshDeviceInfo.ConnectionName}");
        }
        catch (Exception ex)
        {
            Log.Error($"[Config] 保存设备配置失败: {deviceInfo.ConnectionName}, 错误: {ex.Message}");
            throw;
        }
    }

    public async Task RemoveRemoteDeviceAsync(string connectionName)
    {
        try
        {
            Log.Info($"[Config] 开始删除设备配置: {connectionName}");

            // 读取现有配置
            var deviceConfiguration = await LoadTomlConfigurationAsync();

            // 查找要删除的设备
            var deviceIndex = deviceConfiguration.SshDevices.FindIndex(d => 
                d.ConnectionName.Equals(connectionName, StringComparison.OrdinalIgnoreCase));

            if (deviceIndex >= 0)
            {
                deviceConfiguration.SshDevices.RemoveAt(deviceIndex);
                
                // 保存到文件
                await SaveTomlConfigurationAsync(deviceConfiguration);
                
                Log.Info($"[Config] 设备配置删除成功: {connectionName}");
            }
            else
            {
                Log.Warn($"[Config] 未找到要删除的设备配置: {connectionName}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Config] 删除设备配置失败: {connectionName}, 错误: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 加载 TOML 配置文件
    /// </summary>
    private async Task<TomlDeviceConfiguration> LoadTomlConfigurationAsync()
    {
        if (!File.Exists(_configurationPath))
        {
            // 如果文件不存在，创建一个空配置
            return new TomlDeviceConfiguration();
        }

        var tomlContent = await File.ReadAllTextAsync(_configurationPath);
        return TomletMain.To<TomlDeviceConfiguration>(tomlContent);
    }

    /// <summary>
    /// 保存 TOML 配置文件
    /// </summary>
    private async Task SaveTomlConfigurationAsync(TomlDeviceConfiguration configuration)
    {
        // 确保目录存在
        var directory = Path.GetDirectoryName(_configurationPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 转换为 TOML 格式并保存
        var tomlContent = TomletMain.TomlStringFrom(configuration);
        await File.WriteAllTextAsync(_configurationPath, tomlContent);
    }

    /// <summary>
    /// 将 SshRemoteDeviceInfo 转换为 SshDeviceConfiguration
    /// </summary>
    private static SshDeviceConfiguration ConvertToTomlDevice(SshRemoteDeviceInfo sshDeviceInfo)
    {
        return new SshDeviceConfiguration
        {
            ConnectionName = sshDeviceInfo.ConnectionName,
            Host = sshDeviceInfo.Host,
            Port = sshDeviceInfo.Port,
            UserName = sshDeviceInfo.UserName,
            Password = sshDeviceInfo.Password,
            SyncGroups = sshDeviceInfo.SyncGroups.ToList()
        };
    }
}
