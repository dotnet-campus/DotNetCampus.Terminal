# 配置管理专家经验总结

## 必读提醒
🔥 **在开始任何配置相关任务前，必须先阅读本文档！**

## 核心技术栈速查

### TOML解析库
- `Tomlet` - 主要TOML解析库
- `TomlTable` - TOML表格数据结构
- `TomlArray` - TOML数组数据结构

### 配置模型类型
```csharp
// 主配置模型
public record DeviceConfiguration
{
    public List<SshRemoteDeviceInfo> SshDevices { get; set; } = [];
}

// SSH设备配置
public record SshRemoteDeviceInfo : RemoteDeviceInfo
{
    public string LocalId { get; set; }        // 本地唯一标识
    public string? RemoteId { get; set; }      // 远程设备标识
    public string ConnectionName { get; set; }
    public string Host { get; set; }
    public int Port { get; set; } = 22;
    public string UserName { get; set; }
    public string? Password { get; set; }
    public List<SyncGroupConfiguration> SyncGroups { get; set; } = [];
}
```

### 配置服务接口
```csharp
public interface IDeviceConfigurationService
{
    Task<DeviceConfiguration> LoadConfigurationAsync();
    Task SaveConfigurationAsync(DeviceConfiguration configuration);
    string GetConfigurationSourcePath();
}
```

## 设备唯一标识设计

### LocalId生成规则
```csharp
// 生成16位随机标识符，避免重复
private static string GenerateLocalId()
{
    return "device_" + Guid.NewGuid().ToString("N")[..16];
}
```

### 重复设备检测
- 基于 `LocalId` 进行设备去重
- 避免因改名导致重复保存
- 更新现有设备而不是创建新设备

## TOML配置文件格式

### 标准结构
```toml
[[ssh_devices]]
local_id = "device_a1b2c3d4e5f6g7h8"
remote_id = "ubuntu-dev-001"
connection_name = "开发服务器"
host = "192.168.1.100"
port = 22
user_name = "developer"
password = "optional_password"

    [[ssh_devices.sync_groups]]
    name = "项目代码"
    remote_path = "/home/developer/projects"
    local_path = "D:\\Projects"
    enabled = true
```

### 路径处理
- Windows路径使用双反斜杠 `\\` 或正斜杠 `/`
- Linux路径始终使用正斜杠 `/`
- 相对路径自动转换为绝对路径

## 配置持久化策略

### 保存时机
- 用户手动点击"保存"按钮
- 设备信息变更时（通过变更跟踪）
- 程序退出时（可选自动保存）

### TOML源代码保存
```csharp
// 保存原始TOML源代码，便于版本控制
public string TomlSource { get; set; } = string.Empty;

// 序列化为TOML格式
public string SerializeToToml()
{
    return TomletMain.TomlStringFrom(this);
}
```

### 配置文件路径
- 个人配置：`%USERPROFILE%\.dotnetcampus\terminal\config.toml`
- 团队配置：项目根目录 `terminal.toml`
- 配置优先级：个人配置 > 团队配置 > 默认配置

## UI集成要点

### 保存按钮绑定
```csharp
// ViewModel中的保存命令
public AsyncCommand SaveCommand { get; }

// 变更跟踪状态
public bool HasChanges { get; set; }

// 保存状态显示
public bool IsSaving { get; set; }
public string SaveStatus { get; set; } = "";
```

### 表单验证
- 必填字段验证（Host, UserName, ConnectionName）
- 端口范围验证（1-65535）
- 路径有效性验证
- 连接名重复检查

## 错误处理最佳实践

### 配置加载错误
```csharp
try
{
    var config = await LoadConfigurationAsync();
    return config;
}
catch (TomlParseException ex)
{
    Log.Error($"[Config] TOML解析失败: {ex.Message}");
    return new DeviceConfiguration(); // 返回默认配置
}
catch (FileNotFoundException)
{
    Log.Info($"[Config] 配置文件不存在，使用默认配置");
    return new DeviceConfiguration();
}
```

### 配置保存错误
```csharp
try
{
    await SaveConfigurationAsync(config);
    Log.Info($"[Config] 配置保存成功: {configPath}");
}
catch (UnauthorizedAccessException ex)
{
    Log.Error($"[Config] 权限不足，无法保存配置: {ex.Message}");
    throw new ConfigurationException("配置保存失败：权限不足");
}
```

## 常见错误避坑

### ❌ 错误做法
- 忘记设置 `LocalId` 导致重复设备
- 直接使用 `ConnectionName` 作为唯一标识
- 没有处理TOML解析异常
- 忘记转义路径中的反斜杠

### ✅ 正确做法
- 始终为新设备生成 `LocalId`
- 使用 `LocalId` 进行设备去重和更新
- 完善异常处理和降级策略
- 正确处理跨平台路径差异

## 性能优化要点
- 配置文件大小控制（避免存储敏感信息）
- 延迟加载大型配置项
- 配置变更时批量保存
- 使用文件监视器检测外部修改

## 日志记录规范
```csharp
Log.Info($"[Config] 加载配置文件: {configPath}");
Log.Info($"[Config] 发现 {config.SshDevices.Count} 个SSH设备");
Log.Warning($"[Config] 配置文件格式过旧，执行自动迁移");
Log.Error($"[Config] 配置保存失败: {ex.Message}");
```

## 相关知识库文档
- `Terminal-TOML-Configuration-Design.md` - TOML配置设计
- `TOML-Configuration-Implementation-Experience.md` - 实现经验
- `Device-Unique-ID-Design.md` - 设备唯一标识设计
- `Configuration-Save-Feature-Guide.md` - 保存功能指南

---
*最后更新：2025年7月9日*
*下次更新时，请基于实际踩坑经验补充内容*
