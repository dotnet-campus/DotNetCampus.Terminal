# Terminal TOML 设备配置文件设计

> ⚠️ **文档过时警告**: 由于AOT发布限制，TOML配置系统已被弃用。请查看 [TOML到JSON迁移技术方案](./05-TOML到JSON迁移技术方案.md) 了解新的JSON配置系统。

## 设计原则

1. **设备集合源**: TOML 文件是设备集合的配置源，与 DebugSource 和未来可能的数据库源并列
2. **够用就行**: 不过度设计，专注于当前需求
3. **命名一致性**: 采用 PascalCase 与 C# 代码保持一致
4. **用户友好**: 支持注释，易于手动编辑

## 文件结构设计

### 1. SSH 设备配置
```toml
# SSH 设备配置
[[SshDevices]]
ConnectionName = "开发服务器"
Host = "192.168.1.100"
Port = 22
UserName = "developer"
Password = "secret123"  # 可选，建议使用密钥认证

[[SshDevices]]
ConnectionName = "测试服务器"
Host = "test.example.com"
Port = 2222
UserName = "testuser"
# 密码为空，需要在UI中输入

# 同步组配置
[[SshDevices.SyncGroups]]
Name = "项目源码"
RemotePath = "/home/developer/projects/myproject"
LocalPath = "D:\\Projects\\MyProject"
Enabled = true

[[SshDevices.SyncGroups]]
Name = "配置文件"
RemotePath = "/etc/myapp"
LocalPath = "D:\\Config\\MyApp"
Enabled = false
```

### 2. 完整示例
```toml
# DotNetCampus Terminal 设备配置文件

# 第一个SSH设备
[[SshDevices]]
ConnectionName = "开发服务器"
Host = "192.168.1.100"
Port = 22
UserName = "developer"
Password = "secret123"

# 该设备的同步组配置
[[SshDevices.SyncGroups]]
Name = "项目源码"
RemotePath = "/home/developer/projects/myproject"
LocalPath = "D:\\Projects\\MyProject"
Enabled = true

[[SshDevices.SyncGroups]]
Name = "日志文件"
RemotePath = "/var/log/myapp"
LocalPath = "D:\\Logs\\MyApp"
Enabled = false

# 第二个SSH设备
[[SshDevices]]
ConnectionName = "测试服务器"
Host = "test.example.com"
Port = 2222
UserName = "testuser"
# 密码留空，需要在UI中输入

[[SshDevices.SyncGroups]]
Name = "测试数据"
RemotePath = "/home/testuser/testdata"
LocalPath = "D:\\TestData"
Enabled = true
```

## 数据类型映射

### C# 数据模型对应关系
```csharp
// TomlDeviceConfiguration (根对象)
public class TomlDeviceConfiguration
{
    public List<SshDeviceConfiguration> SshDevices { get; set; }
}

// SshDeviceConfiguration
public class SshDeviceConfiguration
{
    public string ConnectionName { get; set; }
    public string Host { get; set; }
    public int Port { get; set; } = 22;
    public string UserName { get; set; }
    public string? Password { get; set; }
    public List<SyncGroupConfiguration> SyncGroups { get; set; }
}

// SyncGroupConfiguration
public class SyncGroupConfiguration
{
    public string Name { get; set; }
    public string RemotePath { get; set; }
    public string LocalPath { get; set; }
    public bool Enabled { get; set; } = true;
}
```

## 字段说明

### 必填字段
- `ConnectionName`: 连接名称，用于UI显示
- `Host`: 主机地址或IP
- `UserName`: 用户名
- `Name`: 同步组名称
- `RemotePath`: 远程路径
- `LocalPath`: 本地路径

### 可选字段
- `Port`: 端口号，默认22
- `Password`: 密码，可留空
- `Enabled`: 同步组是否启用，默认true

## 设计考虑

### 1. 作为配置源
- TOML 文件作为设备集合的配置源，与 DebugSource 并列
- 可以有多个配置源同时工作，比如 TOML 文件 + 数据库 + 远程配置
- 每个配置源负责提供一组设备信息

### 2. 命名一致性
- 采用 PascalCase 与 C# 代码保持一致
- 避免下划线命名，减少转换错误
- 保持与数据模型的直接映射关系

### 3. 安全性
- 密码以明文存储在配置文件中，需要提醒用户注意安全
- 建议使用密钥认证替代密码认证
- 配置文件应设置适当的文件权限

### 4. 可扩展性
- 使用 `[[SshDevices]]` 数组支持多个设备
- 使用嵌套的 `[[SshDevices.SyncGroups]]` 支持每个设备的多个同步组
- 未来可以添加其他设备类型（如 Windows 设备）

## 文件位置

### 默认位置
- **主要位置**: `./Config/devices.toml` (程序配置目录)
- 备用位置: `~/.config/dotnetcampus-terminal/devices.toml`
- 调试位置: `~/Desktop/devices.toml`

### 优先级
1. 程序启动参数指定的配置文件
2. 程序配置目录 (./Config/devices.toml)
3. 用户配置目录
4. 桌面配置文件

## 实现注意事项

### 1. 错误处理
- 配置文件不存在时返回空列表
- 配置文件格式错误时记录日志但不中断程序
- 单个设备配置错误时跳过该设备

### 2. 性能考虑
- 配置文件应缓存解析结果
- 文件变更监控可选实现
- 异步加载避免阻塞UI

### 3. 同步组支持
- 暂时在 TOML 中配置同步组，为后续 FileSync 功能预留
- 同步组配置会传递给 SshRemoteDeviceInfoViewModel
- 实际同步功能由 FileSync 模块实现

## 实现状态

### ✅ 已完成功能
- [x] TOML 配置文件解析和加载
- [x] 设备配置数据模型设计
- [x] 配置源接口和实现
- [x] UI 保存按钮绑定
- [x] 设备配置保存功能
- [x] 同步组配置保存
- [x] 错误处理和日志记录

### 🚧 已知问题
- [ ] **设备唯一标识符缺失**：当前使用设备名称作为标识符，修改名称会导致重复保存
- [ ] **用户反馈不足**：保存成功/失败状态未在UI中显示
- [ ] **并发安全性**：多实例同时修改配置文件可能导致数据丢失

### 📋 后续优化
- [ ] 添加设备唯一ID（GUID）
- [ ] 实现保存状态的UI反馈
- [ ] 添加配置文件备份机制
- [ ] 支持配置文件加密存储
- [ ] 实现团队配置同步

## 安全认证配置升级方案

### SSH密钥认证配置
为了提高安全性，建议从密码认证迁移到密钥认证。配置文件需要支持以下认证方式：

#### 1. 密钥认证配置（推荐）
```toml
[[SshDevices]]
LocalId = "device_a1b2c3d4e5f67890"
ConnectionName = "生产服务器"
Host = "prod.example.com"
Port = 22
UserName = "developer"

# 认证配置
[SshDevices.Auth]
Type = "Key"  # "Password" | "Key" | "Hybrid"
PrivateKeyPath = "~/.ssh/dotnetcampus_terminal_device_a1b2c3d4e5f67890"
PassphraseRequired = false  # 私钥是否有密码短语保护

# 备用认证（可选）
[SshDevices.Auth.Fallback]
Type = "Password"
# Password字段在运行时从安全存储读取，不存储在配置文件中
```

#### 2. 混合认证配置
```toml
[[SshDevices]]
LocalId = "device_b2c3d4e5f6789a01"
ConnectionName = "开发服务器"
Host = "dev.example.com"
Port = 22
UserName = "developer"

[SshDevices.Auth]
Type = "Hybrid"  # 优先尝试密钥，失败时使用密码
PrivateKeyPath = "~/.ssh/dotnetcampus_terminal_device_b2c3d4e5f6789a01"
PassphraseRequired = true
# 密码和密码短语都从安全存储读取
```

#### 3. 安全存储集成
```toml
# 敏感信息存储配置
[Security]
PasswordStorage = "WindowsCredentialManager"  # "WindowsCredentialManager" | "UserSecrets" | "None"
CredentialPrefix = "DotNetCampus.Terminal"

# 不再在配置文件中存储明文密码
# Password = "123"  # 删除此类配置
```

### 配置文件权限要求
- Windows: 文件权限限制为当前用户访问
- Linux/macOS: `chmod 600 terminal.toml`
- 可选：支持整个配置文件加密存储

---

**更新时间**：2025年7月9日  
**实现版本**：v1.0 - 基础保存功能完成
