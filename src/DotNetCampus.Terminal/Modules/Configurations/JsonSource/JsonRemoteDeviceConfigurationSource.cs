using System.Text.Json;
using DotNetCampus.Logging;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Utils;

namespace DotNetCampus.Terminal.Modules.Configurations.JsonSource;

/// <summary>
/// 基于JSON文件的远程设备配置源，支持AOT编译
/// </summary>
public class JsonRemoteDeviceConfigurationSource : IRemoteDeviceConfigurationSource
{
    private readonly string _configurationPath;

    public JsonRemoteDeviceConfigurationSource()
    {
        var basePath = Path.GetDirectoryName(Environment.ProcessPath)!;
        _configurationPath = Path.Combine(basePath, "Configs", "devices.json");
    }

    /// <summary>
    /// 使用指定的配置文件路径
    /// </summary>
    /// <param name="configurationPath">配置文件路径</param>
    public JsonRemoteDeviceConfigurationSource(string configurationPath)
    {
        _configurationPath = configurationPath;
    }

    public string GroupName => "JSON 配置文件";

    public async Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync()
    {
        try
        {
            if (!File.Exists(_configurationPath))
            {
                Log.Info($"[Config] 配置文件不存在，返回空列表: {_configurationPath}");
                return [];
            }

            var jsonContent = await File.ReadAllTextAsync(_configurationPath);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                Log.Info($"[Config] 配置文件为空: {_configurationPath}");
                return [];
            }

            // 创建带注释支持的序列化选项
            var deviceConfiguration = JsonSerializer.Deserialize<DeviceConfiguration>(
                jsonContent,
                AppJsonContext.Default.DeviceConfiguration
            );

            if (deviceConfiguration?.SshDevices == null)
            {
                Log.Warn($"[Config] 配置文件格式无效，未找到SSH设备配置");
                return [];
            }

            Log.Info($"[Config] 成功加载 {deviceConfiguration.SshDevices.Count} 个SSH设备配置");
            return deviceConfiguration.SshDevices.Cast<IRemoteDeviceInfo>().ToList();
        }
        catch (JsonException ex)
        {
            Log.Error($"[Config] JSON配置文件格式错误: {ex.Message}");
            return [];
        }
        catch (Exception ex)
        {
            Log.Error($"[Config] 加载配置文件失败: {ex.Message}");
            return [];
        }
    }

    public async Task SaveRemoteDeviceAsync(IRemoteDeviceInfo deviceInfo)
    {
        if (deviceInfo is not SshRemoteDeviceInfo sshDeviceInfo)
        {
            Log.Error($"[Config] 不支持的设备类型: {deviceInfo.GetType().Name}");
            throw new ArgumentException($"不支持的设备类型: {deviceInfo.GetType().Name}");
        }

        try
        {
            Log.Info($"[Config] 开始保存设备配置: {sshDeviceInfo.ConnectionName} (LocalId: {sshDeviceInfo.LocalId})");

            // 确保LocalId不为空
            if (string.IsNullOrEmpty(sshDeviceInfo.LocalId))
            {
                // 由于SshRemoteDeviceInfo是record类型，我们需要创建一个新的实例
                sshDeviceInfo = sshDeviceInfo with { LocalId = GenerateLocalId() };
                Log.Info($"[Config] 为设备生成LocalId: {sshDeviceInfo.LocalId}");
            }

            // 加载现有配置
            var existingDevices = await FetchRemoteDevicesAsync();
            var deviceList = existingDevices.OfType<SshRemoteDeviceInfo>().ToList();

            // 查找现有设备（基于LocalId）
            var existingIndex = FindExistingDeviceIndex(deviceList, sshDeviceInfo);

            if (existingIndex >= 0)
            {
                // 更新现有设备
                var oldDevice = deviceList[existingIndex];
                deviceList[existingIndex] = sshDeviceInfo;
                Log.Info($"[Config] 更新现有设备配置: {sshDeviceInfo.ConnectionName} (LocalId: {sshDeviceInfo.LocalId}, 原名称: {oldDevice.ConnectionName})");
            }
            else
            {
                // 添加新设备
                deviceList.Add(sshDeviceInfo);
                Log.Info($"[Config] 添加新设备配置: {sshDeviceInfo.ConnectionName} (LocalId: {sshDeviceInfo.LocalId})");
            }

            // 保存配置
            var configuration = new DeviceConfiguration { SshDevices = deviceList };
            await SaveConfigurationAsync(configuration);

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

            // 加载现有配置
            var existingDevices = await FetchRemoteDevicesAsync();
            var deviceList = existingDevices.OfType<SshRemoteDeviceInfo>().ToList();

            // 查找要删除的设备（优先使用LocalId，兼容连接名称）
            var deviceIndex = deviceList.FindIndex(d =>
                d.ConnectionName.Equals(connectionName, StringComparison.OrdinalIgnoreCase));

            if (deviceIndex >= 0)
            {
                var removedDevice = deviceList[deviceIndex];
                deviceList.RemoveAt(deviceIndex);

                // 保存更新后的配置
                var configuration = new DeviceConfiguration { SshDevices = deviceList };
                await SaveConfigurationAsync(configuration);

                Log.Info($"[Config] 设备配置删除成功: {connectionName} (LocalId: {removedDevice.LocalId})");
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
    /// 获取配置文件路径
    /// </summary>
    public string GetConfigurationSourcePath() => _configurationPath;

    #region 私有方法

    /// <summary>
    /// 保存配置到JSON文件
    /// </summary>
    private async Task SaveConfigurationAsync(DeviceConfiguration configuration)
    {
        var jsonContent = JsonSerializer.Serialize(
            configuration,
            ConfigurationJsonContext.Default.DeviceConfiguration
        );

        // 确保目录存在
        var directory = Path.GetDirectoryName(_configurationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(_configurationPath, jsonContent);
        Log.Info($"[Config] 配置文件保存成功: {_configurationPath}");
    }

    /// <summary>
    /// 查找现有设备的索引（基于LocalId进行去重）
    /// </summary>
    private static int FindExistingDeviceIndex(List<SshRemoteDeviceInfo> devices, SshRemoteDeviceInfo targetDevice)
    {
        // 优先使用LocalId进行查找
        if (!string.IsNullOrEmpty(targetDevice.LocalId))
        {
            var index = devices.FindIndex(d =>
                !string.IsNullOrEmpty(d.LocalId) &&
                d.LocalId.Equals(targetDevice.LocalId, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                return index;
            }
        }

        // 兼容性：如果LocalId查找失败，使用连接名称查找
        return devices.FindIndex(d =>
            d.ConnectionName.Equals(targetDevice.ConnectionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 生成16位随机LocalId
    /// </summary>
    private static string GenerateLocalId()
    {
        return "device_" + Guid.NewGuid().ToString("N")[..16];
    }

    #endregion
}
