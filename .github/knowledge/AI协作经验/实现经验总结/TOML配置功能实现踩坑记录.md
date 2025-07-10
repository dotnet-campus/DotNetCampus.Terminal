# TOML 配置解析实现经验总结

> ⚠️ **文档过时警告**: 由于AOT发布限制，TOML配置系统已被弃用。请查看 [TOML到JSON迁移技术方案](../../技术设计文档/配置管理/05-TOML到JSON迁移技术方案.md) 了解新的JSON配置系统。

## 实现过程

### 第一阶段：重新设计配置文件格式
**问题**: 初始设计将 TOML 作为应用程序配置文件，包含了不必要的 app 配置节
**解决方案**: 重新定位 TOML 文件为设备集合配置源，与 DebugSource 并列

### 第二阶段：统一命名规则
**问题**: 配置文件使用 snake_case 命名，与 C# 代码的 PascalCase 不一致
**解决方案**: 采用 PascalCase 命名规则，减少属性映射错误

### 第三阶段：扩展数据模型支持同步组
**问题**: 现有 SshRemoteDeviceInfo 不支持同步组配置
**解决方案**: 扩展数据模型，添加 SyncGroups 属性

## 技术要点

### 1. Tomlet 库使用
```csharp
// 解析 TOML 内容到 C# 对象
var deviceConfiguration = TomletMain.To<TomlDeviceConfiguration>(tomlContent);
```

### 2. 数据模型设计
```csharp
// 专注于设备集合的配置
public class TomlDeviceConfiguration
{
    public List<SshDeviceConfiguration> SshDevices { get; set; } = new();
}
```

### 3. 错误处理模式
```csharp
try
{
    // 解析配置
    return devices;
}
catch (Exception ex)
{
    // 记录错误但不中断程序
    Console.WriteLine($"加载 TOML 配置文件失败: {ex.Message}");
    return [];
}
```

## 配置文件示例

### 完整的 TOML 配置
```toml
# SSH 设备配置
[[SshDevices]]
ConnectionName = "开发服务器"
Host = "192.168.1.100"
Port = 22
UserName = "developer"
Password = "dev123"

# 同步组配置
[[SshDevices.SyncGroups]]
Name = "项目源码"
RemotePath = "/home/developer/projects"
LocalPath = "D:\\Projects"
Enabled = true
```

### 数据流程
```
TOML 文件 → TomlDeviceConfiguration → SshRemoteDeviceInfo → SshRemoteDeviceInfoViewModel
```

## 最佳实践

### 1. 命名一致性
- 使用 PascalCase 与 C# 代码保持一致
- 避免下划线命名，减少映射错误

### 2. 错误处理
- 配置文件不存在时返回空列表
- 解析失败时记录错误但不中断程序
- 单个设备配置错误时跳过该设备

### 3. 扩展性设计
- 使用数组支持多个设备
- 嵌套结构支持设备的多个同步组
- 预留接口支持其他配置源

### 4. 文件位置管理
- 默认从程序目录的 Assets 文件夹加载
- 支持构造函数指定自定义路径
- 可扩展支持多个配置文件路径

## 注意事项

### 1. 同步组配置
- 目前同步组配置存储在 TOML 中，实际同步功能由 FileSync 模块实现
- SyncGroupViewModel 负责 UI 展示和状态管理
- 配置中的 Enabled 字段映射到 SyncGroupStatus

### 2. 安全性考虑
- 密码以明文存储，需要提醒用户注意安全
- 建议使用密钥认证替代密码认证
- 未来可扩展加密存储功能

### 3. 性能优化
- 配置文件解析结果应缓存
- 异步加载避免阻塞UI
- 可考虑文件变更监控

这个实现为项目提供了完整的 TOML 配置解析功能，既满足了当前需求，又保持了良好的扩展性。
