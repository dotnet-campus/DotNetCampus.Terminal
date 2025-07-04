# Tomlet 库使用指南

Tomlet 是一个用于解析和生成 TOML 配置文件的 .NET 库。项目中使用版本：`5.4.0`

## 基本概念

TOML (Tom's Obvious, Minimal Language) 是一种配置文件格式，设计目标是易于阅读和编写。

## 项目中的应用场景

### 1. 应用程序配置
```toml
[app]
name = "DotNetCampus Terminal"
version = "1.0.0"
theme = "dark"
auto_save = true
```

### 2. 设备配置
```toml
[[devices]]
id = "dev-server-1"
name = "开发服务器1"
type = "Linux"
description = "主要开发环境"

[devices.ssh]
host = "192.168.1.100"
port = 22
username = "developer"
private_key_path = "~/.ssh/id_rsa"

[devices.sync]
enabled = true
local_path = "~/projects"
remote_path = "/home/developer/projects"
direction = "local_to_remote"
```

### 3. 团队配置（Git仓库）
```toml
[team]
name = "DotNetCampus开发团队"
repository = "git@github.com:dotnetcampus/terminal-configs.git"

[[team.devices]]
id = "shared-build-server"
name = "共享构建服务器"
type = "Linux"

[team.devices.ssh]
host = "build.dotnetcampus.com"
port = 22
username = "build"
```

## 核心用法

### 1. 反序列化（读取配置）

```csharp
using Tomlet;

// 定义配置类
public class AppConfiguration
{
    public AppInfo App { get; set; } = new();
    public List<DeviceConfiguration> Devices { get; set; } = new();
}

public class AppInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Theme { get; set; } = "dark";
    public bool AutoSave { get; set; } = true;
}

public class DeviceConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SshConfiguration? Ssh { get; set; }
    public SyncConfiguration? Sync { get; set; }
}

public class SshConfiguration
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;
    public string? PrivateKeyPath { get; set; }
    public string? Password { get; set; }
}

public class SyncConfiguration
{
    public bool Enabled { get; set; } = false;
    public string LocalPath { get; set; } = string.Empty;
    public string RemotePath { get; set; } = string.Empty;
    public string Direction { get; set; } = "local_to_remote";
}

// 读取配置文件
public async Task<AppConfiguration> LoadConfigurationAsync(string filePath)
{
    try
    {
        if (!File.Exists(filePath))
        {
            return new AppConfiguration();
        }

        string tomlContent = await File.ReadAllTextAsync(filePath);
        return TomletMain.To<AppConfiguration>(tomlContent);
    }
    catch (Exception ex)
    {
        throw new ConfigurationException($"读取配置文件失败: {ex.Message}", ex);
    }
}
```

### 2. 序列化（保存配置）

```csharp
// 保存配置文件
public async Task SaveConfigurationAsync(AppConfiguration config, string filePath)
{
    try
    {
        string tomlContent = TomletMain.ToTomlString(config);
        
        // 确保目录存在
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllTextAsync(filePath, tomlContent);
    }
    catch (Exception ex)
    {
        throw new ConfigurationException($"保存配置文件失败: {ex.Message}", ex);
    }
}
```

### 3. 配置验证

```csharp
public class ConfigurationValidator
{
    public static void ValidateConfiguration(AppConfiguration config)
    {
        // 验证应用配置
        if (string.IsNullOrWhiteSpace(config.App.Name))
        {
            throw new ConfigurationException("应用名称不能为空");
        }

        // 验证设备配置
        var deviceIds = new HashSet<string>();
        foreach (var device in config.Devices)
        {
            ValidateDevice(device);
            
            if (!deviceIds.Add(device.Id))
            {
                throw new ConfigurationException($"设备ID重复: {device.Id}");
            }
        }
    }

    private static void ValidateDevice(DeviceConfiguration device)
    {
        if (string.IsNullOrWhiteSpace(device.Id))
        {
            throw new ConfigurationException("设备ID不能为空");
        }

        if (string.IsNullOrWhiteSpace(device.Name))
        {
            throw new ConfigurationException($"设备 {device.Id} 的名称不能为空");
        }

        if (device.Ssh != null)
        {
            ValidateSshConfiguration(device.Ssh, device.Id);
        }
    }

    private static void ValidateSshConfiguration(SshConfiguration ssh, string deviceId)
    {
        if (string.IsNullOrWhiteSpace(ssh.Host))
        {
            throw new ConfigurationException($"设备 {deviceId} 的SSH主机地址不能为空");
        }

        if (ssh.Port <= 0 || ssh.Port > 65535)
        {
            throw new ConfigurationException($"设备 {deviceId} 的SSH端口无效: {ssh.Port}");
        }

        if (string.IsNullOrWhiteSpace(ssh.Username))
        {
            throw new ConfigurationException($"设备 {deviceId} 的SSH用户名不能为空");
        }
    }
}

public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message) { }
    public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}
```

## 最佳实践

### 1. 配置文件位置管理

```csharp
public static class ConfigurationPaths
{
    /// <summary>
    /// 用户配置目录
    /// </summary>
    public static string UserConfigDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DotNetCampus.Terminal");

    /// <summary>
    /// 个人设备配置文件路径
    /// </summary>
    public static string PersonalDevicesConfigPath => Path.Combine(
        UserConfigDirectory, "devices.toml");

    /// <summary>
    /// 应用配置文件路径
    /// </summary>
    public static string AppConfigPath => Path.Combine(
        UserConfigDirectory, "app.toml");

    /// <summary>
    /// 团队配置缓存目录
    /// </summary>
    public static string TeamConfigCacheDirectory => Path.Combine(
        UserConfigDirectory, "team-configs");
}
```

### 2. 配置变更监听

```csharp
public class ConfigurationWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly string _configPath;
    private bool _disposed;

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationWatcher(string configPath)
    {
        _configPath = configPath;
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(configPath)!)
        {
            Filter = Path.GetFileName(configPath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };
        
        _watcher.Changed += OnConfigurationFileChanged;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnConfigurationFileChanged(object sender, FileSystemEventArgs e)
    {
        ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(e.FullPath));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _watcher?.Dispose();
            _disposed = true;
        }
    }
}

public class ConfigurationChangedEventArgs : EventArgs
{
    public string FilePath { get; }
    
    public ConfigurationChangedEventArgs(string filePath)
    {
        FilePath = filePath;
    }
}
```

### 3. 配置迁移支持

```csharp
public class ConfigurationMigrator
{
    public static AppConfiguration MigrateConfiguration(string tomlContent, string currentVersion)
    {
        // 尝试解析为当前版本
        try
        {
            return TomletMain.To<AppConfiguration>(tomlContent);
        }
        catch
        {
            // 如果解析失败，尝试旧版本格式
            return MigrateFromLegacyFormat(tomlContent);
        }
    }

    private static AppConfiguration MigrateFromLegacyFormat(string tomlContent)
    {
        // 这里实现旧版本配置的迁移逻辑
        // 例如：字段重命名、结构调整等
        throw new NotImplementedException("配置迁移功能待实现");
    }
}
```

## 常见问题

### 1. TOML 数组表格语法
```toml
# 这是数组表格语法，用于定义多个设备
[[devices]]
id = "server1"
name = "服务器1"

[[devices]]
id = "server2"
name = "服务器2"
```

对应的C#类需要定义为`List<DeviceConfiguration>`。

### 2. 嵌套对象
```toml
[devices.ssh]  # 这种语法在数组表格中不能使用

# 正确的方式是在每个设备块内定义
[[devices]]
id = "server1"

[devices.ssh]
host = "192.168.1.100"
```

### 3. 空值处理
TOML不支持null值，在C#中对应的属性应该使用可空类型：
```csharp
public string? Description { get; set; }  // 可空字符串
public SshConfiguration? Ssh { get; set; }  // 可空对象
```

### 4. 布尔值和数字
```toml
# 布尔值
enabled = true
auto_connect = false

# 数字
port = 22
timeout = 30.5
```

### 5. 特殊字符处理
```toml
# 包含特殊字符的字符串需要引号
name = "My \"Special\" Server"
path = "C:\\Windows\\System32"

# 或使用原始字符串
path = 'C:\Windows\System32'
```

## 性能建议

1. **缓存配置对象** - 避免频繁解析同一配置文件
2. **异步I/O** - 使用异步方法读写文件
3. **配置验证** - 在应用启动时验证配置，而不是每次使用时验证
4. **增量更新** - 只在配置实际改变时重新加载

## 错误处理模式

```csharp
public enum ConfigurationErrorType
{
    FileNotFound,
    ParseError,
    ValidationError,
    IoError
}

public class ConfigurationError
{
    public ConfigurationErrorType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? InnerException { get; set; }
    public string? FilePath { get; set; }
}

public class ConfigurationResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public ConfigurationError? Error { get; set; }

    public static ConfigurationResult<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    public static ConfigurationResult<T> Failure(ConfigurationError error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}
```

这种模式有助于更好地处理配置相关的各种错误情况。
