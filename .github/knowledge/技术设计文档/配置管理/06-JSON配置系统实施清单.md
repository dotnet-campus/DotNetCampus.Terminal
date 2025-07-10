# JSON配置系统实施清单

## 前置条件检查

- [ ] 确认当前项目需要AOT发布
- [ ] 确认Tomlet库确实无法在AOT环境下工作
- [ ] 备份当前所有配置文件和代码

## 阶段1：实现新JSON配置系统

### 1.1 创建JSON源生成器上下文
**文件**: `src/DotNetCampus.Terminal/Modules/Configurations/JsonSource/ConfigurationJsonContext.cs`
```csharp
using System.Text.Json.Serialization;
using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.Modules.Configurations.JsonSource;

[JsonSerializable(typeof(DeviceConfiguration))]
[JsonSerializable(typeof(SshRemoteDeviceInfo))]
[JsonSerializable(typeof(SyncGroupConfiguration))]
[JsonSerializable(typeof(List<SshRemoteDeviceInfo>))]
[JsonSerializable(typeof(List<SyncGroupConfiguration>))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Default,
    UseStringEnumConverter = true
)]
public partial class ConfigurationJsonContext : JsonSerializerContext
{
}
```

### 1.2 实现JSON配置源
**文件**: `src/DotNetCampus.Terminal/Modules/Configurations/JsonSource/JsonRemoteDeviceConfigurationSource.cs`
```csharp
using System.Text.Json;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Logging;

namespace DotNetCampus.Terminal.Modules.Configurations.JsonSource;

public class JsonRemoteDeviceConfigurationSource : IRemoteDeviceConfigurationSource
{
    private readonly string _configurationPath;

    public JsonRemoteDeviceConfigurationSource()
    {
        var basePath = Path.GetDirectoryName(Environment.ProcessPath)!;
        _configurationPath = Path.Combine(basePath, "Configs", "devices.json");
    }

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
                Log.Info("[Config] JSON配置文件不存在，返回空列表");
                return [];
            }

            var jsonContent = await File.ReadAllTextAsync(_configurationPath);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                Log.Info("[Config] JSON配置文件为空，返回空列表");
                return [];
            }

            var deviceConfiguration = JsonSerializer.Deserialize(
                jsonContent, 
                ConfigurationJsonContext.Default.DeviceConfiguration
            );

            if (deviceConfiguration?.SshDevices == null)
            {
                Log.Warn("[Config] JSON配置文件中没有SSH设备配置");
                return [];
            }

            var devices = new List<IRemoteDeviceInfo>();
            foreach (var sshDevice in deviceConfiguration.SshDevices)
            {
                // 确保LocalId不为空
                if (string.IsNullOrWhiteSpace(sshDevice.LocalId))
                {
                    sshDevice.LocalId = GenerateLocalId();
                    Log.Warn($"[Config] 设备 '{sshDevice.ConnectionName}' 缺少LocalId，已自动生成: {sshDevice.LocalId}");
                }

                devices.Add(sshDevice);
            }

            Log.Info($"[Config] 成功加载 {devices.Count} 个设备配置");
            return devices;
        }
        catch (JsonException ex)
        {
            Log.Error($"[Config] JSON配置文件格式错误: {ex.Message}");
            return [];
        }
        catch (Exception ex)
        {
            Log.Error($"[Config] 加载JSON配置文件失败: {ex.Message}");
            return [];
        }
    }

    public async Task SaveRemoteDeviceAsync(IRemoteDeviceInfo deviceInfo)
    {
        if (deviceInfo is not SshRemoteDeviceInfo sshDeviceInfo)
        {
            Log.Error($"[Config] 不支持的设备类型: {deviceInfo.GetType().Name}");
            return;
        }

        try
        {
            Log.Info($"[Config] 开始保存设备配置: {sshDeviceInfo.ConnectionName}");

            // 确保LocalId不为空
            if (string.IsNullOrWhiteSpace(sshDeviceInfo.LocalId))
            {
                sshDeviceInfo.LocalId = GenerateLocalId();
                Log.Info($"[Config] 为设备生成LocalId: {sshDeviceInfo.LocalId}");
            }

            // 加载现有配置
            var existingDevices = await FetchRemoteDevicesAsync();
            var deviceList = existingDevices.OfType<SshRemoteDeviceInfo>().ToList();

            // 检查是否已存在相同LocalId的设备
            var existingIndex = deviceList.FindIndex(d => d.LocalId == sshDeviceInfo.LocalId);
            if (existingIndex >= 0)
            {
                deviceList[existingIndex] = sshDeviceInfo;
                Log.Info($"[Config] 更新现有设备配置: {sshDeviceInfo.ConnectionName}");
            }
            else
            {
                deviceList.Add(sshDeviceInfo);
                Log.Info($"[Config] 添加新设备配置: {sshDeviceInfo.ConnectionName}");
            }

            // 保存配置
            var configuration = new DeviceConfiguration { SshDevices = deviceList };
            await SaveConfigurationAsync(configuration);

            Log.Info($"[Config] 设备配置保存成功: {sshDeviceInfo.ConnectionName}");
        }
        catch (Exception ex)
        {
            Log.Error($"[Config] 保存设备配置失败: {ex.Message}");
            throw;
        }
    }

    private async Task SaveConfigurationAsync(DeviceConfiguration configuration)
    {
        var tempPath = _configurationPath + ".tmp";

        try
        {
            var jsonContent = JsonSerializer.Serialize(
                configuration, 
                ConfigurationJsonContext.Default.DeviceConfiguration
            );

            // 确保目录存在
            var directory = Path.GetDirectoryName(_configurationPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 原子性保存：先写入临时文件，再替换
            await File.WriteAllTextAsync(tempPath, jsonContent);
            File.Move(tempPath, _configurationPath, overwrite: true);

            Log.Info($"[Config] 配置文件保存成功: {_configurationPath}");
        }
        catch (Exception ex)
        {
            // 清理临时文件
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // 忽略清理失败
                }
            }

            Log.Error($"[Config] 保存配置文件失败: {ex.Message}");
            throw;
        }
    }

    private static string GenerateLocalId()
    {
        return "device_" + Guid.NewGuid().ToString("N")[..16];
    }
}
```

### 1.3 更新配置管理器
**文件**: `src/DotNetCampus.Terminal/Modules/Configurations/ConfigurationManager.cs`
```csharp
// 在构造函数中更新配置源列表
private readonly List<IRemoteDeviceConfigurationSource> _remoteDeviceSources =
[
    new JsonRemoteDeviceConfigurationSource(),  // 新的JSON源
    new TomlRemoteDeviceConfigurationSource(),  // 保留TOML源用于迁移
];

// 添加迁移检查方法
public async Task<bool> PerformMigrationIfNeededAsync()
{
    var migrationService = new ConfigurationMigrationService();
    
    if (await migrationService.NeedsMigrationAsync())
    {
        Log.Info("[Config] 检测到需要迁移TOML配置到JSON格式");
        
        var result = await migrationService.MigrateAsync();
        if (result.Success)
        {
            Log.Info($"[Config] 配置迁移成功，迁移了 {result.MigratedDevices} 个设备");
            if (!string.IsNullOrWhiteSpace(result.BackupPath))
            {
                Log.Info($"[Config] 原配置文件已备份到: {result.BackupPath}");
            }
            return true;
        }
        else
        {
            Log.Error($"[Config] 配置迁移失败: {result.ErrorMessage}");
            return false;
        }
    }
    
    return true; // 无需迁移
}
```

## 阶段2：实现配置迁移工具

### 2.1 创建迁移服务
**文件**: `src/DotNetCampus.Terminal/Modules/Configurations/Migration/ConfigurationMigrationService.cs`
```csharp
using System.Text.Json;
using DotNetCampus.Terminal.Modules.Configurations.JsonSource;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Modules.Configurations.TomlSource;
using DotNetCampus.Logging;
using Tomlet;

namespace DotNetCampus.Terminal.Modules.Configurations.Migration;

public class ConfigurationMigrationService
{
    private readonly string _tomlPath;
    private readonly string _jsonPath;

    public ConfigurationMigrationService()
    {
        var basePath = Path.GetDirectoryName(Environment.ProcessPath)!;
        var configDir = Path.Combine(basePath, "Configs");
        _tomlPath = Path.Combine(configDir, "devices.toml");
        _jsonPath = Path.Combine(configDir, "devices.json");
    }

    public async Task<bool> NeedsMigrationAsync()
    {
        return File.Exists(_tomlPath) && !File.Exists(_jsonPath);
    }

    public async Task<MigrationResult> MigrateAsync()
    {
        try
        {
            Log.Info("[Config] 开始配置迁移: TOML → JSON");

            // 1. 读取TOML配置
            var tomlContent = await File.ReadAllTextAsync(_tomlPath);
            var tomlConfig = TomletMain.To<TomlDeviceConfiguration>(tomlContent);

            // 2. 转换为新模型
            var jsonConfig = new DeviceConfiguration
            {
                SshDevices = tomlConfig.SshDevices.Select(ConvertSshDevice).ToList()
            };

            // 3. 保存JSON配置
            var jsonContent = JsonSerializer.Serialize(
                jsonConfig, 
                ConfigurationJsonContext.Default.DeviceConfiguration
            );

            // 确保目录存在
            var directory = Path.GetDirectoryName(_jsonPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(_jsonPath, jsonContent);

            // 4. 备份原文件
            var backupPath = _tomlPath + $".backup.{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(_tomlPath, backupPath, overwrite: true);

            Log.Info("[Config] 配置迁移完成");

            return new MigrationResult 
            { 
                Success = true, 
                MigratedDevices = jsonConfig.SshDevices.Count,
                BackupPath = backupPath
            };
        }
        catch (Exception ex)
        {
            Log.Error($"[Config] 配置迁移失败: {ex.Message}");
            return new MigrationResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            };
        }
    }

    private SshRemoteDeviceInfo ConvertSshDevice(SshDeviceConfiguration tomlDevice)
    {
        return new SshRemoteDeviceInfo
        {
            LocalId = tomlDevice.LocalId,
            RemoteId = tomlDevice.RemoteId,
            ConnectionName = tomlDevice.ConnectionName,
            Host = tomlDevice.Host,
            Port = tomlDevice.Port,
            UserName = tomlDevice.UserName,
            Password = tomlDevice.Password,
            SyncGroups = tomlDevice.SyncGroups ?? []
        };
    }
}

public record MigrationResult
{
    public bool Success { get; init; }
    public int MigratedDevices { get; init; }
    public string? BackupPath { get; init; }
    public string? ErrorMessage { get; init; }
}
```

### 2.2 在应用启动时调用迁移
**文件**: `src/DotNetCampus.Terminal/Program.cs` 或 `App.axaml.cs`
```csharp
// 在应用启动时进行迁移检查
public override void OnFrameworkInitializationCompleted()
{
    // ...现有代码...

    // 执行配置迁移
    var configManager = serviceProvider.GetRequiredService<ConfigurationManager>();
    var migrationSuccess = await configManager.PerformMigrationIfNeededAsync();
    
    if (!migrationSuccess)
    {
        // 处理迁移失败的情况
        Log.Warn("[Config] 配置迁移失败，将使用默认配置");
    }

    // ...现有代码...
}
```

## 阶段3：测试验证

### 3.1 编译测试
```bash
dotnet build
```

### 3.2 创建测试配置文件
创建 `Configs/devices.json` 测试文件：
```json
{
  "sshDevices": [
    {
      "localId": "test_device_001",
      "connectionName": "测试服务器",
      "host": "127.0.0.1",
      "port": 22,
      "userName": "test",
      "syncGroups": []
    }
  ]
}
```

### 3.3 运行测试
```bash
dotnet run
```

### 3.4 验证配置加载
- 检查日志输出是否正确
- 验证设备信息是否正确加载
- 测试保存功能是否正常

## 阶段4：清理TOML依赖

### 4.1 移除TOML包引用
**文件**: `src/DotNetCampus.Terminal/DotNetCampus.Terminal.csproj`
```xml
<!-- 移除这一行 -->
<PackageReference Include="Samboy063.Tomlet" />
```

### 4.2 删除TOML相关文件
```bash
# 删除这些文件
rm src/DotNetCampus.Terminal/Modules/Configurations/TomlSource/TomlRemoteDeviceConfigurationSource.cs
rm src/DotNetCampus.Terminal/Modules/Configurations/Models/TomlDeviceConfiguration.cs
rm -rf src/DotNetCampus.Terminal/Modules/Configurations/TomlSource/
```

### 4.3 更新配置管理器
**文件**: `src/DotNetCampus.Terminal/Modules/Configurations/ConfigurationManager.cs`
```csharp
// 移除TOML源，只保留JSON源
private readonly List<IRemoteDeviceConfigurationSource> _remoteDeviceSources =
[
    new JsonRemoteDeviceConfigurationSource(),
];
```

### 4.4 清理迁移代码
删除 `ConfigurationMigrationService` 相关代码（可选，建议保留一段时间）

## 实施注意事项

### 编译错误处理
- 如果遇到JSON序列化错误，检查所有类型是否都在`JsonSerializable`中声明
- 确保使用正确的`ConfigurationJsonContext`进行序列化/反序列化

### 配置文件权限
- 确保应用对`Configs`目录有读写权限
- 在Windows上可能需要管理员权限

### 数据完整性
- 在迁移前务必备份原配置文件
- 验证迁移后的数据是否完整
- 提供回滚机制

### 性能考虑
- JSON序列化比TOML更快
- 源生成器避免运行时反射，提升性能
- 考虑大配置文件的异步加载

## 验收标准

- [ ] 应用能够正常启动和关闭
- [ ] 配置文件加载正常，无错误日志
- [ ] 设备信息显示正确
- [ ] 保存功能正常工作
- [ ] 迁移功能正常工作（如果有现有TOML文件）
- [ ] AOT发布能够正常工作
- [ ] 性能测试通过（加载时间、内存使用）

## 回滚计划

如果出现严重问题，可以按以下步骤回滚：

1. 恢复TOML包引用
2. 恢复TOML相关代码文件
3. 恢复原配置管理器代码
4. 从备份文件恢复原配置
5. 重新编译和测试

---

**重要提醒**: 每完成一个阶段都要进行编译测试，确保没有引入新的错误。遇到问题及时记录到知识库中。
