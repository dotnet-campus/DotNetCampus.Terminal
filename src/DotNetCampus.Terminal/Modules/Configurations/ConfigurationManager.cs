using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Modules.Configurations.TomlSource;
using DotNetCampus.Logging;

namespace DotNetCampus.Terminal.Modules.Configurations;

public class ConfigurationManager
{
    private readonly List<IRemoteDeviceConfigurationSource> _remoteDeviceSources =
    [
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

    /// <summary>
    /// 保存远程设备配置
    /// </summary>
    /// <param name="deviceInfo">要保存的设备信息</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task SaveRemoteDeviceAsync(IRemoteDeviceInfo deviceInfo)
    {
        try
        {
            Log.Info($"[Config] 开始保存设备配置: {deviceInfo.ConnectionName}");

            // 目前只支持 SSH 设备，所以直接使用 TOML 源保存
            // 未来如果有其他类型的设备或源，可以根据设备类型选择合适的源
            if (deviceInfo is SshRemoteDeviceInfo)
            {
                var tomlSource = _remoteDeviceSources.OfType<TomlRemoteDeviceConfigurationSource>().FirstOrDefault();
                if (tomlSource != null)
                {
                    await tomlSource.SaveRemoteDeviceAsync(deviceInfo);
                    Log.Info($"[Config] 设备配置保存成功: {deviceInfo.ConnectionName}");
                }
                else
                {
                    Log.Error("[Config] 未找到 TOML 配置源");
                    throw new InvalidOperationException("未找到 TOML 配置源");
                }
            }
            else
            {
                Log.Error($"[Config] 不支持的设备类型: {deviceInfo.GetType().Name}");
                throw new NotSupportedException($"不支持的设备类型: {deviceInfo.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Config] 保存设备配置失败: {deviceInfo.ConnectionName}, 错误: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 删除远程设备配置
    /// </summary>
    /// <param name="connectionName">设备连接名称</param>
    /// <param name="deviceType">设备类型</param>
    /// <returns>表示异步操作的任务</returns>
    public async Task RemoveRemoteDeviceAsync(string connectionName, RemoteDeviceType deviceType = RemoteDeviceType.LinuxSsh)
    {
        try
        {
            Log.Info($"[Config] 开始删除设备配置: {connectionName}");

            // 目前只支持 SSH 设备，所以直接使用 TOML 源删除
            if (deviceType == RemoteDeviceType.LinuxSsh)
            {
                var tomlSource = _remoteDeviceSources.OfType<TomlRemoteDeviceConfigurationSource>().FirstOrDefault();
                if (tomlSource != null)
                {
                    await tomlSource.RemoveRemoteDeviceAsync(connectionName);
                    Log.Info($"[Config] 设备配置删除成功: {connectionName}");
                }
                else
                {
                    Log.Error("[Config] 未找到 TOML 配置源");
                    throw new InvalidOperationException("未找到 TOML 配置源");
                }
            }
            else
            {
                Log.Error($"[Config] 不支持的设备类型: {deviceType}");
                throw new NotSupportedException($"不支持的设备类型: {deviceType}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Config] 删除设备配置失败: {connectionName}, 错误: {ex.Message}");
            throw;
        }
    }
}

public record RemoteDeviceGroup
{
    public required string Name { get; init; }

    public required IReadOnlyList<IRemoteDeviceInfo> Devices { get; init; } = [];
}
