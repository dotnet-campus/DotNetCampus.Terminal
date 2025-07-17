using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Modules.Configurations.JsonSource;
using DotNetCampus.Logging;

namespace DotNetCampus.Terminal.Modules.Configurations;

public class ConfigurationManager
{
    private readonly List<IRemoteDeviceConfigurationSource> _remoteDeviceSources =
    [
        new JsonRemoteDeviceConfigurationSource(),
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

            if (!_remoteDeviceSources.Any())
            {
                Log.Error("[Config] 未找到可用的配置源");
                throw new InvalidOperationException("未找到可用的配置源");
            }

            // 保存到所有配置源
            var saveResults = new List<Task>();
            foreach (var configSource in _remoteDeviceSources)
            {
                saveResults.Add(configSource.SaveRemoteDeviceAsync(deviceInfo));
            }

            await Task.WhenAll(saveResults);
            Log.Info($"[Config] 设备配置保存成功到 {_remoteDeviceSources.Count} 个配置源: {deviceInfo.ConnectionName}");
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
    /// <returns>表示异步操作的任务</returns>
    public async Task RemoveRemoteDeviceAsync(string connectionName)
    {
        try
        {
            Log.Info($"[Config] 开始删除设备配置: {connectionName}");

            if (!_remoteDeviceSources.Any())
            {
                Log.Error("[Config] 未找到可用的配置源");
                throw new InvalidOperationException("未找到可用的配置源");
            }

            // 从所有配置源删除
            var removeResults = new List<Task>();
            foreach (var configSource in _remoteDeviceSources)
            {
                removeResults.Add(configSource.RemoveRemoteDeviceAsync(connectionName));
            }

            await Task.WhenAll(removeResults);
            Log.Info($"[Config] 设备配置删除成功从 {_remoteDeviceSources.Count} 个配置源: {connectionName}");
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
