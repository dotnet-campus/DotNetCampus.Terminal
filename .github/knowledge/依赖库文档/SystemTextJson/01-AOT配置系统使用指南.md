# System.Text.Json AOT配置系统使用指南

## 概述

本文档描述基于System.Text.Json和源生成器的AOT兼容配置系统，用于替代原有的TOML配置系统。

## 核心组件

### 1. JSON源生成器上下文

```csharp
using System.Text.Json.Serialization;

[JsonSerializable(typeof(DeviceConfiguration))]
[JsonSerializable(typeof(SshRemoteDeviceInfo))]
[JsonSerializable(typeof(SyncGroupConfiguration))]
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

**关键特性说明**：
- `WriteIndented = true`: 生成格式化的JSON，便于手工编辑
- `PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase`: 使用驼峰命名
- `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`: 忽略null值
- `GenerationMode = JsonSourceGenerationMode.Default`: 完整功能模式
- `UseStringEnumConverter = true`: 枚举序列化为字符串

### 2. JSON配置源实现

```csharp
public class JsonRemoteDeviceConfigurationSource : IRemoteDeviceConfigurationSource
{
    private readonly string _configurationPath;
    
    public JsonRemoteDeviceConfigurationSource()
    {
        var basePath = Path.GetDirectoryName(Environment.ProcessPath)!;
        _configurationPath = Path.Combine(basePath, "Configs", "devices.json");
    }

    public string GroupName => "JSON 配置文件";

    public async Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync()
    {
        try
        {
            if (!File.Exists(_configurationPath))
                return [];

            var jsonContent = await File.ReadAllTextAsync(_configurationPath);
            if (string.IsNullOrWhiteSpace(jsonContent))
                return [];

            var deviceConfiguration = JsonSerializer.Deserialize(
                jsonContent, 
                ConfigurationJsonContext.Default.DeviceConfiguration
            );

            return deviceConfiguration?.SshDevices?.Cast<IRemoteDeviceInfo>().ToList() ?? [];
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
            return;
        }

        try
        {
            // 加载现有配置
            var existingDevices = await FetchRemoteDevicesAsync();
            var deviceList = existingDevices.OfType<SshRemoteDeviceInfo>().ToList();

            // 更新或添加设备
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
            var jsonContent = JsonSerializer.Serialize(
                configuration, 
                ConfigurationJsonContext.Default.DeviceConfiguration
            );
            
            Directory.CreateDirectory(Path.GetDirectoryName(_configurationPath)!);
            await File.WriteAllTextAsync(_configurationPath, jsonContent);
            
            Log.Info($"[Config] 配置文件保存成功: {_configurationPath}");
        }
        catch (Exception ex)
        {
            Log.Error($"[Config] 保存配置文件失败: {ex.Message}");
            throw;
        }
    }
}
```

## 配置文件格式

### JSON配置文件示例 (devices.json)

```json
{
  "sshDevices": [
    {
      "localId": "device_a1b2c3d4e5f6g7h8",
      "remoteId": "ubuntu-dev-001",
      "connectionName": "开发服务器",
      "host": "192.168.1.100",
      "port": 22,
      "userName": "developer",
      "password": "optional_password",
      "syncGroups": [
        {
          "name": "项目代码",
          "remotePath": "/home/developer/projects",
          "localPath": "D:\\Projects",
          "isEnabled": true,
          "syncDirection": "RemoteToLocal",
          "excludePatterns": [
            "*.tmp",
            "node_modules/",
            ".git/"
          ]
        },
        {
          "name": "配置文件",
          "remotePath": "/etc/myapp",
          "localPath": "D:\\Config\\MyApp",
          "isEnabled": false,
          "syncDirection": "RemoteToLocal"
        }
      ]
    },
    {
      "localId": "device_b9c8d7e6f5g4h3i2",
      "connectionName": "测试服务器",
      "host": "test.example.com",
      "port": 2222,
      "userName": "testuser",
      "syncGroups": []
    }
  ]
}
```

### 配置文件结构说明

- **sshDevices**: SSH设备配置数组
  - **localId**: 本地唯一标识符（必需）
  - **remoteId**: 远程设备标识符（可选）
  - **connectionName**: 连接显示名称（必需）
  - **host**: 主机地址（必需）
  - **port**: SSH端口（默认22）
  - **userName**: 用户名（必需）
  - **password**: 密码（可选，建议使用密钥认证）
  - **syncGroups**: 同步组配置数组
    - **name**: 同步组名称
    - **remotePath**: 远程路径
    - **localPath**: 本地路径
    - **isEnabled**: 是否启用同步
    - **syncDirection**: 同步方向
    - **excludePatterns**: 排除模式（可选）

## AOT兼容性要点

### 1. 源生成器配置

```csharp
// 必须标记所有需要序列化的类型
[JsonSerializable(typeof(DeviceConfiguration))]
[JsonSerializable(typeof(SshRemoteDeviceInfo))]
[JsonSerializable(typeof(SyncGroupConfiguration))]
[JsonSerializable(typeof(List<SshRemoteDeviceInfo>))]
[JsonSerializable(typeof(List<SyncGroupConfiguration>))]
```

### 2. 序列化调用

```csharp
// 正确的AOT兼容调用方式
var config = JsonSerializer.Deserialize(
    jsonContent, 
    ConfigurationJsonContext.Default.DeviceConfiguration
);

var jsonContent = JsonSerializer.Serialize(
    configuration, 
    ConfigurationJsonContext.Default.DeviceConfiguration
);
```

### 3. 避免的反射调用

```csharp
// ❌ 错误：在AOT中不可用
var config = JsonSerializer.Deserialize<DeviceConfiguration>(jsonContent);

// ❌ 错误：使用了反射
var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions 
{ 
    WriteIndented = true 
});
```

## 错误处理模式

### 1. 配置文件损坏处理

```csharp
public async Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync()
{
    try
    {
        // 正常加载逻辑
        return deviceConfiguration?.SshDevices?.Cast<IRemoteDeviceInfo>().ToList() ?? [];
    }
    catch (JsonException ex)
    {
        Log.Error($"[Config] JSON配置文件格式错误: {ex.Message}");
        
        // 可选：尝试创建备份并重置为默认配置
        await BackupCorruptedConfigAsync();
        return [];
    }
    catch (Exception ex)
    {
        Log.Error($"[Config] 加载配置文件失败: {ex.Message}");
        return [];
    }
}

private async Task BackupCorruptedConfigAsync()
{
    try
    {
        var backupPath = _configurationPath + $".corrupted.{DateTime.Now:yyyyMMddHHmmss}";
        File.Copy(_configurationPath, backupPath);
        Log.Info($"[Config] 已备份损坏的配置文件到: {backupPath}");
    }
    catch (Exception ex)
    {
        Log.Warn($"[Config] 备份损坏配置文件失败: {ex.Message}");
    }
}
```

### 2. 保存操作原子性

```csharp
public async Task SaveRemoteDeviceAsync(IRemoteDeviceInfo deviceInfo)
{
    var tempPath = _configurationPath + ".tmp";
    
    try
    {
        // 先写入临时文件
        var jsonContent = JsonSerializer.Serialize(
            configuration, 
            ConfigurationJsonContext.Default.DeviceConfiguration
        );
        
        await File.WriteAllTextAsync(tempPath, jsonContent);
        
        // 原子性替换
        File.Move(tempPath, _configurationPath, overwrite: true);
        
        Log.Info($"[Config] 配置文件保存成功");
    }
    catch (Exception ex)
    {
        // 清理临时文件
        if (File.Exists(tempPath))
            File.Delete(tempPath);
            
        Log.Error($"[Config] 保存配置文件失败: {ex.Message}");
        throw;
    }
}
```

## 性能优化

### 1. 缓存序列化选项

```csharp
public class JsonRemoteDeviceConfigurationSource
{
    private static readonly JsonTypeInfo<DeviceConfiguration> ConfigTypeInfo = 
        ConfigurationJsonContext.Default.DeviceConfiguration;
    
    public async Task<DeviceConfiguration?> LoadConfigurationAsync()
    {
        var jsonContent = await File.ReadAllTextAsync(_configurationPath);
        return JsonSerializer.Deserialize(jsonContent, ConfigTypeInfo);
    }
}
```

### 2. 异步流处理大文件

```csharp
public async Task<DeviceConfiguration?> LoadConfigurationStreamAsync()
{
    using var fileStream = new FileStream(_configurationPath, FileMode.Open, FileAccess.Read);
    return await JsonSerializer.DeserializeAsync(fileStream, ConfigTypeInfo);
}
```

## 配置验证

### 1. 数据模型验证

```csharp
public static class ConfigurationValidator
{
    public static ValidationResult ValidateConfiguration(DeviceConfiguration config)
    {
        var result = new ValidationResult();
        
        if (config?.SshDevices == null)
        {
            result.AddError("配置文件不能为空");
            return result;
        }

        foreach (var device in config.SshDevices)
        {
            ValidateDevice(device, result);
        }

        return result;
    }

    private static void ValidateDevice(SshRemoteDeviceInfo device, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(device.LocalId))
            result.AddError($"设备 '{device.ConnectionName}' 缺少本地ID");
            
        if (string.IsNullOrWhiteSpace(device.ConnectionName))
            result.AddError("设备缺少连接名称");
            
        if (string.IsNullOrWhiteSpace(device.Host))
            result.AddError($"设备 '{device.ConnectionName}' 缺少主机地址");
            
        if (device.Port <= 0 || device.Port > 65535)
            result.AddError($"设备 '{device.ConnectionName}' 端口号无效: {device.Port}");
    }
}
```

## 迁移工具集成

### 配置管理器更新

```csharp
public class ConfigurationManager
{
    private readonly List<IRemoteDeviceConfigurationSource> _remoteDeviceSources =
    [
        new JsonRemoteDeviceConfigurationSource(), // 新的JSON源
        // 迁移期间可以同时保留TOML源进行兼容
    ];

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
                Log.Info($"[Config] 原配置文件已备份到: {result.BackupPath}");
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
}
```

## 常见问题排查

### 1. 序列化错误
**问题**: `JsonException: The JSON value could not be converted`
**解决**: 检查JSON格式和数据模型属性类型是否匹配

### 2. AOT编译错误
**问题**: 运行时找不到序列化方法
**解决**: 确保所有类型都在`JsonSerializable`中声明

### 3. 配置文件权限错误
**问题**: `UnauthorizedAccessException`
**解决**: 检查配置目录权限，确保应用有读写权限

### 4. 配置文件损坏
**问题**: JSON格式错误
**解决**: 使用配置验证工具检查和修复

## 最佳实践

1. **始终使用源生成器上下文进行序列化**
2. **实现原子性保存操作**
3. **添加配置验证逻辑**
4. **妥善处理错误和异常**
5. **定期备份配置文件**
6. **使用结构化日志记录操作**
