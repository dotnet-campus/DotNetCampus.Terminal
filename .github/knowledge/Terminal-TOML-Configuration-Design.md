# Terminal TOML 设备配置文件设计

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
- 程序目录: `./Assets/terminal.toml`
- 用户配置: `~/.config/dotnetcampus-terminal/terminal.toml`
- 桌面配置: `~/Desktop/terminal.toml`

### 优先级
1. 程序启动参数指定的配置文件
2. 用户配置目录
3. 程序目录配置文件
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

这个设计确保了 TOML 配置文件作为设备集合源的正确定位，同时保持了与代码的一致性和良好的可扩展性。
