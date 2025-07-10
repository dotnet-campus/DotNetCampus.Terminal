# TOML到JSON配置系统迁移技术方案

## 迁移背景

由于项目采用AOT发布，TOML解析库`Tomlet`依赖反射无法在AOT环境下工作。因此需要将配置系统从TOML格式迁移到支持AOT的JSON格式+源生成器方案。

## 技术方案选择

**选定方案**: System.Text.Json + 源生成器
- ✅ 微软官方推荐的AOT序列化方案
- ✅ 编译时生成序列化代码，运行时零反射
- ✅ 性能优异，JSON格式广泛支持
- ✅ 迁移路径清晰，工具丰富

## 迁移架构设计

### 1. 新配置系统架构

```csharp
// 配置模型保持不变
public record DeviceConfiguration
{
    public List<SshRemoteDeviceInfo> SshDevices { get; set; } = [];
}

// JSON源生成器上下文
[JsonSerializable(typeof(DeviceConfiguration))]
[JsonSerializable(typeof(SshRemoteDeviceInfo))]
[JsonSerializable(typeof(SyncGroupConfiguration))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    GenerationMode = JsonSourceGenerationMode.Default
)]
public partial class ConfigurationJsonContext : JsonSerializerContext
{
}
```

### 2. 新配置源实现

```csharp
public class JsonRemoteDeviceConfigurationSource : IRemoteDeviceConfigurationSource
{
    private readonly string _configurationPath;
    
    public JsonRemoteDeviceConfigurationSource()
    {
        var basePath = Path.GetDirectoryName(Environment.ProcessPath)!;
        _configurationPath = Path.Combine(basePath, "Configs", "devices.json");
    }

    public async Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync()
    {
        if (!File.Exists(_configurationPath))
            return [];

        var jsonContent = await File.ReadAllTextAsync(_configurationPath);
        var deviceConfiguration = JsonSerializer.Deserialize(
            jsonContent, 
            ConfigurationJsonContext.Default.DeviceConfiguration
        );

        return deviceConfiguration?.SshDevices.Cast<IRemoteDeviceInfo>().ToList() ?? [];
    }

    public async Task SaveRemoteDeviceAsync(IRemoteDeviceInfo deviceInfo)
    {
        // 加载现有配置
        var existingDevices = await FetchRemoteDevicesAsync();
        var deviceList = existingDevices.OfType<SshRemoteDeviceInfo>().ToList();

        // 更新或添加设备
        var existingIndex = deviceList.FindIndex(d => d.LocalId == deviceInfo.LocalId);
        if (existingIndex >= 0)
        {
            deviceList[existingIndex] = (SshRemoteDeviceInfo)deviceInfo;
        }
        else
        {
            deviceList.Add((SshRemoteDeviceInfo)deviceInfo);
        }

        // 保存配置
        var configuration = new DeviceConfiguration { SshDevices = deviceList };
        var jsonContent = JsonSerializer.Serialize(
            configuration, 
            ConfigurationJsonContext.Default.DeviceConfiguration
        );
        
        Directory.CreateDirectory(Path.GetDirectoryName(_configurationPath)!);
        await File.WriteAllTextAsync(_configurationPath, jsonContent);
    }
}
```

## 配置文件格式对比

### 现有TOML格式
```toml
[[SshDevices]]
LocalId = "device_a1b2c3d4e5f6g7h8"
RemoteId = "ubuntu-dev-001"
ConnectionName = "开发服务器"
Host = "192.168.1.100"
Port = 22
UserName = "developer"
Password = "optional_password"

[[SshDevices.SyncGroups]]
Name = "项目代码"
RemotePath = "/home/developer/projects"
LocalPath = "D:\\Projects"
IsEnabled = true
```

### 新JSON格式
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
          "isEnabled": true
        }
      ]
    }
  ]
}
```

## 实施步骤

### 阶段1：新系统实现
1. **创建JSON源生成器上下文** - 定义`ConfigurationJsonContext`
2. **实现JSON配置源** - 新增`JsonRemoteDeviceConfigurationSource`
3. **更新配置管理器** - 在`ConfigurationManager`中添加JSON源支持
4. **配置文件迁移工具** - 实现TOML→JSON转换工具

### 阶段2：迁移现有配置
1. **检测现有配置** - 检查是否存在`devices.toml`文件
2. **自动迁移** - 首次启动时自动将TOML配置转换为JSON
3. **备份原文件** - 保留原TOML文件作为备份
4. **验证迁移结果** - 确保配置数据完整性

### 阶段3：清理TOML依赖
1. **移除TOML源** - 删除`TomlRemoteDeviceConfigurationSource`
2. **移除依赖库** - 从项目中移除`Samboy063.Tomlet`包引用
3. **清理代码** - 删除TOML相关的模型和工具类
4. **更新文档** - 更新配置相关文档

## 配置迁移工具设计

```csharp
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
            await File.WriteAllTextAsync(_jsonPath, jsonContent);

            // 4. 备份原文件
            var backupPath = _tomlPath + ".backup";
            File.Copy(_tomlPath, backupPath, overwrite: true);

            return new MigrationResult 
            { 
                Success = true, 
                MigratedDevices = jsonConfig.SshDevices.Count,
                BackupPath = backupPath
            };
        }
        catch (Exception ex)
        {
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
            SyncGroups = tomlDevice.SyncGroups
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

## 性能和兼容性

### AOT兼容性保证
- 使用`JsonSourceGenerationMode.Default`确保完整的序列化支持
- 编译时生成所有必要的序列化代码
- 避免运行时反射调用

### 序列化性能优化
```csharp
[JsonSourceGenerationOptions(
    WriteIndented = true,                    // 格式化输出，便于手工编辑
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,  // 使用camelCase命名
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,  // 忽略null值
    GenerationMode = JsonSourceGenerationMode.Default,      // 完整功能模式
    UseStringEnumConverter = true           // 枚举使用字符串表示
)]
```

### 错误处理策略
```csharp
public async Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync()
{
    try
    {
        if (!File.Exists(_configurationPath))
            return [];

        var jsonContent = await File.ReadAllTextAsync(_configurationPath);
        
        // 验证JSON格式
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
        // 可以考虑尝试修复或创建默认配置
        return [];
    }
    catch (Exception ex)
    {
        Log.Error($"[Config] 加载配置文件失败: {ex.Message}");
        return [];
    }
}
```

## 实施注意事项

### 1. 数据模型兼容性
- 保持现有的`IRemoteDeviceInfo`接口不变
- 确保`SshRemoteDeviceInfo`属性完全兼容
- 维护设备唯一标识符逻辑

### 2. 配置文件位置
- 新文件路径：`Configs/devices.json`
- 保持与现有TOML文件相同的目录结构
- 支持相对路径和绝对路径

### 3. 迁移安全性
- 在迁移前创建备份
- 验证迁移后的数据完整性
- 提供回滚机制（保留原TOML文件）

### 4. 用户体验
- 首次启动时自动检测并迁移
- 提供迁移进度反馈
- 迁移完成后提示用户

## 测试验证点

### 单元测试
- JSON序列化/反序列化测试
- 配置迁移工具测试
- 错误处理场景测试

### 集成测试
- 完整配置流程测试
- AOT发布兼容性测试
- 多设备配置场景测试

### 性能测试
- 大配置文件加载性能
- 序列化性能对比
- 内存使用情况

## 技术债务处理

### 需要移除的组件
1. `TomlRemoteDeviceConfigurationSource.cs`
2. `TomlDeviceConfiguration.cs`  
3. `SshDeviceConfiguration.cs`
4. `Samboy063.Tomlet`包引用
5. TOML相关的知识库文档

### 需要更新的文档
1. 配置管理专家经验总结
2. 技术设计文档
3. 用户使用指南
4. API文档

## 实施优先级

### 高优先级（必须完成）
- [ ] 创建JSON源生成器上下文
- [ ] 实现JSON配置源
- [ ] 实现配置迁移工具
- [ ] 更新配置管理器

### 中优先级（建议完成）  
- [ ] 完善错误处理和日志
- [ ] 添加配置验证
- [ ] 性能优化
- [ ] 单元测试

### 低优先级（后续优化）
- [ ] 配置加密支持
- [ ] 配置版本管理
- [ ] 自动备份策略
- [ ] 配置共享功能

---

**下一步行动**: 请具体实施的AI按照此方案分阶段实现，首先从阶段1开始，确保每个步骤都经过编译验证。
