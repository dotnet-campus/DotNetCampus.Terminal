# JSON配置系统架构设计

## 系统概述

DotNetCampus Terminal 使用基于 `System.Text.Json` + 源生成器的AOT兼容配置系统，完全替代了之前的TOML配置方案。

## 📜 配置系统演进历史

### TOML时代 (已废弃)
**时间**: 项目初期 - 2025年7月10日  
**技术方案**: `Samboy063.Tomlet` 库 + TOML格式  
**废弃原因**:
- 🔥 **AOT兼容性问题**: Tomlet库依赖反射，无法在AOT环境下工作
- 🔥 **官方推荐**: Microsoft推荐使用 System.Text.Json + 源生成器
- 🔥 **性能考虑**: JSON + 源生成器性能更优，编译时生成代码

### JSON时代 (当前)
**时间**: 2025年7月10日 - 至今  
**技术方案**: `System.Text.Json` + 源生成器 + JSON格式  
**优势**:
- ✅ AOT完全兼容
- ✅ 编译时生成序列化代码，运行时零反射
- ✅ Microsoft官方推荐方案
- ✅ 广泛的工具链支持

## 架构设计

### 核心组件

```csharp
// 1. 配置根对象
public record DeviceConfiguration
{
    public List<SshRemoteDeviceInfo> SshDevices { get; set; } = [];
}

// 2. AOT源生成器上下文
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

// 3. JSON配置源实现
public class JsonRemoteDeviceConfigurationSource : IRemoteDeviceConfigurationSource
{
    // 实现配置的读取和保存
}
```

### 设计原则

1. **AOT优先**: 所有序列化使用源生成器，避免运行时反射
2. **接口隔离**: 通过 `IRemoteDeviceConfigurationSource` 接口隔离具体实现
3. **简化设计**: 直接替换配置系统，无需复杂的迁移工具
4. **向前兼容**: 保持与现有 `SshRemoteDeviceInfo` 模型的兼容性

## 文件结构

### 配置文件位置
```
Configs/
└── devices.json          # 设备配置文件
```

### 源代码组织
```
Modules/Configurations/
├── IRemoteDeviceConfigurationSource.cs      # 配置源接口
├── JsonRemoteDeviceConfigurationSource.cs   # JSON配置源实现
├── ConfigurationJsonContext.cs              # AOT源生成器上下文
├── ConfigurationManager.cs                  # 简化接口调用
└── Models/
    ├── DeviceConfiguration.cs               # 配置根对象
    └── SyncModels.cs                        # 同步相关模型
```

## JSON配置示例

```json
{
  "sshDevices": [
    {
      "connectionName": "开发服务器",
      "host": "192.168.1.100",
      "port": 22,
      "userName": "developer",
      "password": "dev123",
      "syncGroups": [
        {
          "name": "项目源码",
          "remotePath": "/home/developer/projects",
          "localPath": "D:\\Projects",
          "enabled": true,
          "direction": "RemoteToLocal"
        }
      ]
    }
  ]
}
```

## AOT序列化配置

### 源生成器选项
```csharp
[JsonSourceGenerationOptions(
    WriteIndented = true,                    // 格式化输出
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,  // 驼峰命名
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,  // 忽略null值
    GenerationMode = JsonSourceGenerationMode.Default  // 默认生成模式
)]
```

### 性能优势
- **编译时生成**: 序列化代码在编译时生成，无运行时开销
- **零反射**: 完全避免运行时反射调用
- **AOT兼容**: 支持原生AOT编译和发布
- **类型安全**: 编译时类型检查，减少运行时错误

## 接口设计

### IRemoteDeviceConfigurationSource
```csharp
public interface IRemoteDeviceConfigurationSource
{
    Task<List<SshRemoteDeviceInfo>> GetDevicesAsync();
    Task SaveDevicesAsync(List<SshRemoteDeviceInfo> devices);
}
```

### ConfigurationManager
```csharp
public class ConfigurationManager
{
    private readonly IRemoteDeviceConfigurationSource _configurationSource;
    
    // 简化接口调用，无需类型转换
    public async Task<List<SshRemoteDeviceInfo>> LoadDevicesAsync()
    public async Task SaveDevicesAsync(List<SshRemoteDeviceInfo> devices)
}
```

## 最佳实践

### 1. 模型设计
- 使用 `record` 类型简化数据模型
- 添加 `JsonPropertyName` 属性确保序列化一致性
- 为集合属性提供默认空集合初始值

### 2. 错误处理
```csharp
try
{
    var json = await File.ReadAllTextAsync(_configurationPath);
    var config = JsonSerializer.Deserialize(json, ConfigurationJsonContext.Default.DeviceConfiguration);
    return config?.SshDevices ?? [];
}
catch (Exception ex)
{
    Log.Warn($"[Config] 加载配置文件失败: {ex.Message}");
    return [];
}
```

### 3. 性能优化
- 使用 `JsonSerializer.Deserialize` 配合源生成器上下文
- 避免频繁的文件I/O操作
- 合理设置JSON序列化选项

## 未来扩展

### 配置迁移
- 如需支持配置格式升级，可在 `ConfigurationManager` 中添加版本检测逻辑
- 保持向后兼容性，支持渐进式迁移

### 多环境配置
- 可扩展支持开发/生产环境配置文件
- 通过环境变量或命令行参数选择配置源

### 配置验证
- 添加配置模型验证逻辑
- 在加载时检查必要字段和格式有效性
