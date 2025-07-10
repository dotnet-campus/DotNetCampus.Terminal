using System.Text.Json;
using DotNetCampus.Logging;
using DotNetCampus.Terminal.Modules.Configurations.JsonSource;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Modules.Configurations.TomlSource;

namespace DotNetCampus.Terminal.Modules.Configurations;

/// <summary>
/// 配置迁移工具，用于从TOML配置迁移到JSON配置
/// </summary>
public class ConfigurationMigrationTool
{
    private readonly TomlRemoteDeviceConfigurationSource _tomlSource;
    private readonly JsonRemoteDeviceConfigurationSource _jsonSource;

    public ConfigurationMigrationTool()
    {
        _tomlSource = new TomlRemoteDeviceConfigurationSource();
        _jsonSource = new JsonRemoteDeviceConfigurationSource();
    }

    /// <summary>
    /// 迁移TOML配置到JSON配置
    /// </summary>
    /// <returns>是否需要迁移以及迁移结果</returns>
    public async Task<MigrationResult> MigrateToJsonAsync()
    {
        try
        {
            Log.Info("[Migration] 开始配置迁移：TOML → JSON");

            // 检查TOML配置是否存在
            var tomlPath = _tomlSource.GetConfigurationSourcePath();
            if (!File.Exists(tomlPath))
            {
                Log.Info("[Migration] TOML配置文件不存在，无需迁移");
                return new MigrationResult { IsRequired = false, Success = true, Message = "TOML配置文件不存在，无需迁移" };
            }

            // 检查JSON配置是否已存在
            var jsonPath = _jsonSource.GetConfigurationSourcePath();
            if (File.Exists(jsonPath))
            {
                Log.Info("[Migration] JSON配置文件已存在，跳过迁移");
                return new MigrationResult { IsRequired = false, Success = true, Message = "JSON配置文件已存在，跳过迁移" };
            }

            // 从TOML加载配置
            var tomlDevices = await _tomlSource.FetchRemoteDevicesAsync();
            if (!tomlDevices.Any())
            {
                Log.Info("[Migration] TOML配置文件为空，创建空的JSON配置");
                await CreateEmptyJsonConfigurationAsync();
                return new MigrationResult { IsRequired = true, Success = true, Message = "TOML配置为空，已创建空的JSON配置" };
            }

            // 迁移每个设备
            var migratedCount = 0;
            foreach (var device in tomlDevices.OfType<SshRemoteDeviceInfo>())
            {
                // 确保设备有LocalId
                var deviceToMigrate = EnsureLocalId(device);
                await _jsonSource.SaveRemoteDeviceAsync(deviceToMigrate);
                migratedCount++;
                Log.Info($"[Migration] 已迁移设备: {device.ConnectionName}");
            }

            // 备份TOML配置文件
            await BackupTomlConfigurationAsync(tomlPath);

            Log.Info($"[Migration] 配置迁移完成，共迁移 {migratedCount} 个设备");
            return new MigrationResult 
            { 
                IsRequired = true, 
                Success = true, 
                Message = $"配置迁移完成，共迁移 {migratedCount} 个设备",
                MigratedDeviceCount = migratedCount
            };
        }
        catch (Exception ex)
        {
            Log.Error($"[Migration] 配置迁移失败: {ex.Message}");
            return new MigrationResult 
            { 
                IsRequired = true, 
                Success = false, 
                Message = $"配置迁移失败: {ex.Message}",
                Error = ex
            };
        }
    }

    /// <summary>
    /// 检查是否需要迁移
    /// </summary>
    public bool IsMigrationRequired()
    {
        var tomlPath = _tomlSource.GetConfigurationSourcePath();
        var jsonPath = _jsonSource.GetConfigurationSourcePath();
        
        return File.Exists(tomlPath) && !File.Exists(jsonPath);
    }

    private async Task CreateEmptyJsonConfigurationAsync()
    {
        var emptyConfiguration = new DeviceConfiguration();
        var jsonContent = JsonSerializer.Serialize(
            emptyConfiguration, 
            ConfigurationJsonContext.Default.DeviceConfiguration
        );
        
        var jsonPath = _jsonSource.GetConfigurationSourcePath();
        var directory = Path.GetDirectoryName(jsonPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllTextAsync(jsonPath, jsonContent);
        Log.Info($"[Migration] 创建空的JSON配置文件: {jsonPath}");
    }

    private static SshRemoteDeviceInfo EnsureLocalId(SshRemoteDeviceInfo device)
    {
        if (!string.IsNullOrEmpty(device.LocalId))
        {
            return device;
        }

        var localId = "device_" + Guid.NewGuid().ToString("N")[..16];
        return device with { LocalId = localId };
    }

    private async Task BackupTomlConfigurationAsync(string tomlPath)
    {
        try
        {
            var backupPath = tomlPath + ".backup." + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            await File.WriteAllTextAsync(backupPath, await File.ReadAllTextAsync(tomlPath));
            Log.Info($"[Migration] TOML配置已备份到: {backupPath}");
        }
        catch (Exception ex)
        {
            Log.Warn($"[Migration] 备份TOML配置失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 配置迁移结果
/// </summary>
public class MigrationResult
{
    /// <summary>
    /// 是否需要迁移
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 迁移是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 结果消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 迁移的设备数量
    /// </summary>
    public int MigratedDeviceCount { get; set; }

    /// <summary>
    /// 迁移过程中的错误（如果有）
    /// </summary>
    public Exception? Error { get; set; }
}
