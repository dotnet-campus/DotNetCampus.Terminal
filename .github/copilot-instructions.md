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

### 1. 视图结构和命名空间
```xml
<!-- 基础命名空间 -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:console="https://github.com/jinek/consolonia"
             xmlns:vm="using:DotNetCampus.Terminal.ViewModels">
```

### 2. 应用程序配置
```xml
<!-- App.axaml 主题配置 -->
<Application RequestedThemeVariant="Dark">
    <Application.Styles>
        <console:TurboVisionDarkTheme />
    </Application.Styles>
</Application>
```

### 3. 主要布局模式
```xml
<!-- 主-详细页模式 -->
<Grid ColumnDefinitions="*,2*">
    <TreeView Grid.Column="0" ItemsSource="{Binding Items}" />
    <ContentControl Grid.Column="1" Content="{Binding SelectedItem}" />
</Grid>

<!-- 状态栏模式 -->
<Grid RowDefinitions="*,Auto">
    <TabControl Grid.Row="0" />
    <StackPanel Grid.Row="1" Orientation="Horizontal">
        <!-- 功能键 -->
    </StackPanel>
</Grid>
```

### 4. 控件样式规范
```xml
<!-- 按钮样式 -->
<Style Selector="Button">
    <Setter Property="console:ButtonExtensions.Shadow" Value="False" />
    <Style Selector="^ /template/ Border#InternalBorder">
        <Setter Property="Margin" Value="0" />
    </Style>
</Style>

<!-- 边框样式 -->
<Border BorderThickness="1">
    <Border.BorderBrush>
        <console:LineBrush LineStyle="EdgeWide" Brush="DimGray" />
    </Border.BorderBrush>
</Border>
```

### 5. 数据绑定最佳实践
```xml
<!-- 绑定模式选择 -->
<TextBlock Text="{Binding ReadOnlyProperty, Mode=OneTime}" />  <!-- 不变数据 -->
<TextBlock Text="{Binding StatusProperty, Mode=OneWay}" />     <!-- 状态数据 -->
<TextBox Text="{Binding InputProperty, Mode=TwoWay}" />        <!-- 用户输入 -->
```

### 6. 转换器模式
```csharp
// 泛型转换器基类
public class StateToObjectConverter<T> : IValueConverter where T : class
{
    public T? Online { get; set; }
    public T? Offline { get; set; }
    public T? Default { get; set; }
}

// 特化转换器
public class StateToBrushConverter : StateToObjectConverter<IBrush>;
```

### 7. MVVM 模式
```csharp
// 使用 BindableRecord 作为 ViewModel 基类
public record DeviceViewModel : BindableRecord
{
    private ConnectionState _state;
    
    public ConnectionState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }
}

// 使用 AvaloniaList 而不是 ObservableCollection
public AvaloniaList<DeviceNode> Devices { get; } = [];
```

### 8. 异步操作和UI更新
```csharp
// 确保UI更新在UI线程
await Dispatcher.UIThread.InvokeAsync(() =>
{
    ConnectionState = ConnectionState.Online;
});

// 异步命令模式
public AsyncCommand ConnectCommand { get; }
```

### 9. 虚拟化性能优化
```xml
<TreeView.ItemsPanel>
    <ItemsPanelTemplate>
        <VirtualizingStackPanel />
    </ItemsPanelTemplate>
</TreeView.ItemsPanel>
```

### 10. 数据模板切换
```xml
<ContentControl Content="{Binding SelectedItem}">
    <ContentControl.DataTemplates>
        <DataTemplate x:DataType="vm:DeviceNode">
            <views:DeviceView />
        </DataTemplate>
        <DataTemplate x:DataType="vm:GroupNode">
            <views:GroupView />
        </DataTemplate>
    </ContentControl.DataTemplates>
</ContentControl>
```

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
#### 应用程序启动
- 使用 `UseConsolonia()` 启用框架
- 配置 `UseAutoDetectedConsole()` 和 `UseAutoDetectConsoleColorMode()`
- 集成依赖注入 `UseContainerServices()`

#### 样式问题
- 检查 `console:` 命名空间是否正确引用
- 确认 `TurboVisionDarkTheme` 是否正确应用
- 验证选择器语法，特别是模板选择器 `^ /template/`

#### 数据绑定问题
- 确保 ViewModel 继承自 `BindableRecord`
- 使用 `AvaloniaList<T>` 而非 `ObservableCollection<T>`
- 检查 `x:DataType` 是否正确设置

#### 性能问题
- 使用 `VirtualizingStackPanel` 处理大数据集
- 正确设置绑定模式（OneTime/OneWay/TwoWay）
- 避免不必要的UI线程调用

#### 布局问题
- 检查 Grid 的 RowDefinitions 和 ColumnDefinitions
- 确认控件的 Grid.Row 和 Grid.Column 属性
- 验证 Margin 和 Padding 设置

#### 控件不显示
- 检查 DataTemplate 的 x:DataType 是否匹配
- 确认 DataContext 是否正确设置
- 验证 ItemsSource 绑定是否正确

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

#### 核心组件和用法
- **应用程序配置**: 使用 `TurboVisionDarkTheme` 主题，支持深色模式
- **布局控件**: Grid, StackPanel, TreeView, TabControl 等，支持控制台优化
- **数据绑定**: 完全支持 MVVM，使用 `AvaloniaList<T>` 替代 `ObservableCollection<T>`
- **样式系统**: 使用 CSS-like 选择器，支持伪类和模板选择器
- **控制台特效**: `console:LineBrush` 用于边框，`console:ButtonExtensions` 用于按钮控制

#### 关键开发模式
- **文件扩展名**: 使用 `.axaml` 而不是 `.xaml`
- **命名空间**: `xmlns:console="https://github.com/jinek/consolonia"`
- **缩放控制**: 使用 `LayoutTransformControl` 和 `ScaleTransform`
- **虚拟化**: 使用 `VirtualizingStackPanel` 优化大数据集性能
- **异步UI更新**: 使用 `Dispatcher.UIThread.InvokeAsync`

#### 常见控件使用
- **TreeView**: 支持分层数据展示，使用 `TreeDataTemplate` 和 `DataTemplates`
- **ContentControl**: 实现视图切换，结合 `DataTemplate` 实现类型到视图的映射
- **TabControl**: 标签页容器，支持动态内容切换
- **按钮**: 禁用阴影效果 `console:ButtonExtensions.Shadow="False"`

#### 性能优化要点
- 使用合适的绑定模式（OneTime/OneWay/TwoWay）
- 实现虚拟化滚动处理大数据集
- 避免频繁的UI线程调用
- 使用转换器优化数据转换逻辑

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
