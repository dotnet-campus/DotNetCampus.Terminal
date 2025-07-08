# GitHub Copilot 指导文档

这是 DotNetCampus Terminal 项目的 GitHub Copilot 指导文档，旨在确保所有AI按照统一的标准和方式工作。

## 项目概述

DotNetCampus Terminal 是一个基于 .NET 9.0 的远程设备连接管理工具，使用 Consolonia 构建控制台UI界面。项目采用多AI协同开发模式。

## 技术栈和依赖

- **.NET 9.0** - 目标框架
- **Consolonia** - 控制台UI框架
- **SSH.NET** - SSH连接库
- **Tomlet** - TOML配置文件解析
- **C# 12** - 编程语言，启用nullable引用类型

## 编码规范

### 1. 命名约定
- **类名**：PascalCase，如 `ConfigurationManager`
- **方法名**：PascalCase，如 `GetDeviceList()`
- **属性名**：PascalCase，如 `DeviceName`
- **字段名**：camelCase，私有字段使用下划线前缀，如 `_deviceList`
- **接口名**：以I开头，如 `IDeviceManager`
- **常量**：PascalCase，如 `DefaultTimeout`

### 2. 文件和目录结构
```
src/DotNetCampus.Terminal/
├── Models/           # 数据模型
├── Interfaces/       # 接口定义
├── Services/         # 业务服务
├── Views/           # UI视图
├── ViewModels/      # 视图模型
├── Framework/       # 框架代码
├── Configurations/ # 配置管理
├── SshManagement/  # SSH连接管理
├── FileSync/       # 文件同步
└── ProcessManagement/ # 进程管理
```

### 3. 代码风格
- 使用 **nullable 引用类型**，明确标注可空性
- 优先使用 **异步编程**，方法名以Async结尾
- 使用 **依赖注入**，避免静态依赖
- 遵循 **SOLID 原则**
- 使用 **XML 文档注释**

### 4. 示例代码模板

#### 接口定义
```csharp
/// <summary>
/// 设备管理接口
/// </summary>
public interface IDeviceManager
{
    /// <summary>
    /// 异步获取设备列表
    /// </summary>
    /// <returns>设备列表</returns>
    Task<IReadOnlyList<Device>> GetDevicesAsync();
    
    /// <summary>
    /// 异步添加设备
    /// </summary>
    /// <param name="device">要添加的设备</param>
    /// <returns>操作结果</returns>
    Task<bool> AddDeviceAsync(Device device);
}
```

#### 服务实现
```csharp
/// <summary>
/// 设备管理服务实现
/// </summary>
public class DeviceManager : IDeviceManager
{
    private readonly IConfigurationManager _configurationManager;
    private readonly ILogger<DeviceManager> _logger;

    public DeviceManager(
        IConfigurationManager configurationManager,
        ILogger<DeviceManager> logger)
    {
        _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<Device>> GetDevicesAsync()
    {
        try
        {
            _logger.LogInformation("开始获取设备列表");
            // 实现逻辑
            return new List<Device>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设备列表失败");
            throw;
        }
    }
}
```

#### 数据模型
```csharp
/// <summary>
/// 设备信息模型
/// </summary>
public record Device
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 设备名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public required DeviceType Type { get; init; }

    /// <summary>
    /// SSH配置（可选）
    /// </summary>
    public SshConfiguration? SshConfig { get; init; }
}
```

## 错误处理规范

### 1. 异常处理
- 使用具体的异常类型，避免通用Exception
- 提供有意义的错误消息
- 记录详细的错误日志
- 在适当的层级处理异常

### 2. 结果模式
对于可能失败的操作，考虑使用Result模式：

```csharp
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
```

## Consolonia 使用规范

### 1. 视图结构
```csharp
// 待补充：Consolonia 视图结构示例
// 此部分需要知识学习者重新学习 Consolonia 后填写
```

### 2. 数据绑定
- 使用Observable模式更新UI
- 避免直接在UI线程进行耗时操作
- 合理使用数据绑定机制

## 测试规范

### 1. 单元测试
```csharp
[Test]
public async Task GetDevicesAsync_ShouldReturnDeviceList()
{
    // Arrange
    var configManager = new Mock<IConfigurationManager>();
    var logger = new Mock<ILogger<DeviceManager>>();
    var deviceManager = new DeviceManager(configManager.Object, logger.Object);

    // Act
    var result = await deviceManager.GetDevicesAsync();

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result, Is.Empty);
}
```

### 2. 集成测试
- 测试完整的用户场景
- 使用TestContainers进行SSH连接测试
- 模拟真实的配置环境

## 配置管理规范

### 1. 配置文件格式（TOML）
```toml
[app]
name = "DotNetCampus Terminal"
version = "1.0.0"

[[devices]]
id = "dev-server-1"
name = "开发服务器1"
type = "Linux"

[devices.ssh]
host = "192.168.1.100"
port = 22
username = "developer"
```

### 2. 配置类定义
```csharp
public class AppConfiguration
{
    public AppInfo App { get; set; } = new();
    public List<DeviceConfiguration> Devices { get; set; } = new();
}

public class AppInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
```

## 日志规范

### 1. 日志级别使用
- **Trace**: 详细的调试信息
- **Debug**: 调试信息
- **Information**: 一般信息，重要的程序流程
- **Warning**: 警告，程序可以继续运行
- **Error**: 错误，但程序可以继续
- **Critical**: 严重错误，程序可能需要停止

### 2. 日志消息格式
```csharp
_logger.LogInformation("开始连接到设备 {DeviceName} ({DeviceHost})", device.Name, device.Host);
_logger.LogWarning("设备 {DeviceId} 连接超时，正在重试 ({RetryCount}/{MaxRetries})", deviceId, retryCount, maxRetries);
_logger.LogError(ex, "连接到设备 {DeviceId} 失败", deviceId);
```

## Git 提交规范

### 1. 提交消息格式
```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### 2. 类型说明
- **feat**: 新功能
- **fix**: 修复bug
- **docs**: 文档更新
- **style**: 代码格式修改
- **refactor**: 重构
- **test**: 测试相关
- **chore**: 构建过程或辅助工具变动

### 3. 示例
```
feat(ssh): 添加SSH连接重试机制

- 实现指数退避重试策略
- 添加最大重试次数配置
- 改善连接失败的错误处理

Closes #123
```

## AI 协作指南

### 1. 开始工作前
- 查看 `.github/AI任务分工.md` 确认自己的职责
- 检查依赖的接口是否已定义
- 与相关AI协调接口设计

### 2. 代码实现中
- 遵循上述所有编码规范
- 及时提交进度，便于其他AI跟进
- 遇到架构问题时咨询架构师AI

### 3. 完成后
- 编写完整的单元测试
- 更新相关文档
- 通知依赖此模块的其他AI

## 常见问题解决

### 1. Consolonia 相关
- 待补充：Consolonia 使用要点和常见问题
- 此部分需要知识学习者重新学习 Consolonia 后填写

### 2. SSH.NET 相关
- 正确处理连接超时和异常
- 使用连接池避免频繁创建连接
- 实现心跳检测保持连接活跃

### 3. 文件操作
- 使用异步文件操作API
- 正确处理文件锁定和权限问题
- 实现进度回调用于大文件操作

## 性能优化指南

### 1. 内存管理
- 及时释放大对象
- 使用 `IDisposable` 模式
- 避免不必要的字符串拼接

### 2. 异步操作
- 使用 `ConfigureAwait(false)` 在库代码中
- 避免阻塞UI线程
- 合理使用并发限制

### 3. 缓存策略
- 缓存频繁访问的配置
- 实现智能的缓存失效机制
- 注意缓存的内存占用

这个指导文档会随着项目发展持续更新，请所有AI定期查看最新版本。

## AI 协作开发经验总结

### 1. Consolonia 开发要点

**重要提示**: 详细的 Consolonia 使用指南待知识学习者重新学习后完善

基本要点：
- 待补充：Consolonia 基本使用要点
- 此部分需要知识学习者重新学习 Consolonia 后填写

### 2. 错误处理策略

#### 求助时机
以下情况建议立即寻求人类帮助，避免 AI 陷入错误循环：
- 多个命名空间冲突同时出现
- API 版本兼容性问题
- 复杂的泛型推断失败
- 平台特定的显示问题

### 3. 协作效率提升

#### 知识复用原则
- **开发前必读**: 先查看 `.github/knowledge/` 中的相关技术文档
- **问题先查**: 遇到问题先查阅知识库中的解决方案
- **及时更新**: 将新发现的问题和解决方案及时更新到知识库
- **经验分享**: 在代码注释中说明特殊处理的原因

#### 渐进式开发流程
1. **架构优先**: 先设计接口和数据结构
2. **功能分层**: 按 UI -> 业务逻辑 -> 数据访问 的顺序开发
3. **及时测试**: 每完成一个模块立即进行编译和基本功能测试
4. **知识积累**: 将开发过程中的经验及时记录到知识库

#### 沟通机制
- **依赖明确**: 在开始工作前检查依赖模块的接口状态
- **变更通知**: 接口变更时主动通知相关 AI
- **进度同步**: 定期更新 AI任务分工.md 中的任务状态
- **知识共享**: 技术问题解决后及时更新到相应的知识文档

这些经验总结来自实际开发过程中的问题和解决方案，具体的技术细节请参考 `.github/knowledge/` 目录下的专门文档。
