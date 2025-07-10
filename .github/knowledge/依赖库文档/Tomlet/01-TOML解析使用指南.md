# Tomlet 库使用指南

> ⚠️ **文档过时警告**: 由于AOT发布限制，Tomlet库已被弃用。请查看 [TOML到JSON迁移技术方案](../../技术设计文档/配置管理/05-TOML到JSON迁移技术方案.md) 了解新的JSON配置系统。

Tomlet 是一个零依赖的 .NET TOML 解析库，完全实现 TOML 1.0.0 规范。

## 核心 API

### 主要入口点
```csharp
// TomletMain - 主要API类
TomletMain.To<T>(string tomlString)           // 字符串 → 对象
TomletMain.To<T>(TomlValue value)             // TomlValue → 对象
TomletMain.TomlStringFrom<T>(T obj)           // 对象 → 字符串
TomletMain.DocumentFrom<T>(T obj)             // 对象 → TomlDocument

// TomlParser - 解析器
var parser = new TomlParser();
TomlDocument doc = parser.Parse(tomlString);   // 字符串 → TomlDocument
TomlDocument doc = TomlParser.ParseFile(path); // 文件 → TomlDocument
```

### 基本用法
```csharp
// 1. 反序列化
var config = TomletMain.To<AppConfig>(tomlString);

// 2. 序列化
string toml = TomletMain.TomlStringFrom(config);

// 3. 文件操作
var doc = TomlParser.ParseFile("config.toml");
var config = TomletMain.To<AppConfig>(doc);
```

## 属性映射

### 基本属性特性
```csharp
public class DeviceConfig
{
    [TomlProperty("device_name")]     // 映射到不同的TOML键名
    public string Name { get; set; }
    
    [TomlNonSerialized]              // 跳过序列化
    public string TempData { get; set; }
    
    [TomlDoNotInlineObject]          // 强制不内联表格
    public SshConfig Ssh { get; set; }
}
```

### 自定义序列化器
```csharp
// 注册自定义类型映射
TomletMain.RegisterMapper<Color>(
    // 序列化：Color → TomlValue
    color => new TomlString($"#{color.R:X2}{color.G:X2}{color.B:X2}"),
    // 反序列化：TomlValue → Color
    value => Color.FromArgb(/* 解析十六进制 */)
);
```

## 项目中的数据模型映射

### SSH 设备配置
```csharp
public class SshRemoteDeviceInfo
{
    public string ConnectionName { get; set; }
    public string Host { get; set; }
    public int Port { get; set; } = 22;
    public string UserName { get; set; }
    public string? Password { get; set; }
}

// 对应 TOML
/*
connection_name = "开发服务器"
host = "192.168.1.100"
port = 22
user_name = "developer"
password = "secret"
*/
```

### 同步组配置
```csharp
public class SyncGroupConfig
{
    public string Name { get; set; }
    public string RemotePath { get; set; }
    public string LocalPath { get; set; }
    public bool Enabled { get; set; } = true;
}

// 对应 TOML 数组
/*
[[sync_groups]]
name = "项目代码"
remote_path = "/home/dev/projects"
local_path = "D:\\Projects"
enabled = true
*/
```

## 关键语法要点

### 1. 表格数组语法
```toml
# 单个表格
[ssh]
host = "server1"

# 表格数组 - 对应 List<T>
[[devices]]
name = "Server1"

[[devices]]
name = "Server2"
```

### 2. 嵌套表格
```toml
# 正确的嵌套
[[devices]]
name = "Server1"

[devices.ssh]    # 属于最后一个 devices 条目
host = "server1"
```

### 3. 数据类型
```toml
# 基本类型
string_val = "text"
int_val = 123
float_val = 12.34
bool_val = true
datetime_val = 2025-07-08T10:30:00Z

# 数组
array_val = ["item1", "item2"]
```

## 错误处理

### 常见异常类型
- `TomlException` - 基础异常
- `TomlParseException` - 解析错误
- `TomlTypeMismatchException` - 类型不匹配
- `TomlKeyRedefinitionException` - 键重复定义

### 错误处理模式
```csharp
public static class ConfigLoader
{
    public static Result<T> LoadConfig<T>(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return Result<T>.Failure("配置文件不存在");
                
            var content = File.ReadAllText(filePath);
            var config = TomletMain.To<T>(content);
            return Result<T>.Success(config);
        }
        catch (TomlException ex)
        {
            return Result<T>.Failure($"TOML解析错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"读取配置失败: {ex.Message}");
        }
    }
}
```

## 性能要点

1. **缓存解析结果** - 避免重复解析相同配置
2. **使用异步I/O** - 文件操作使用异步方法
3. **合理使用TomlDocument** - 中间格式便于多次转换
4. **避免过度嵌套** - 深层嵌套影响性能

## 项目集成示例

```csharp
// 配置管理器实现
public class TomlConfigurationSource : IRemoteDeviceConfigurationSource
{
    private readonly string _filePath;
    
    public async Task<IReadOnlyList<IRemoteDeviceInfo>> FetchRemoteDevicesAsync()
    {
        if (!File.Exists(_filePath))
            return Array.Empty<IRemoteDeviceInfo>();
            
        var content = await File.ReadAllTextAsync(_filePath);
        var config = TomletMain.To<DeviceConfig>(content);
        
        return config.Devices.Cast<IRemoteDeviceInfo>().ToList();
    }
}
```

这个指南涵盖了 Tomlet 在项目中的核心使用方法，重点关注实际应用场景。
