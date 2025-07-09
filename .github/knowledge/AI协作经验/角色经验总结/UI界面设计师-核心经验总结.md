# UI界面设计师经验总结

## 必读提醒
🔥 **在开始任何UI相关任务前，必须先阅读本文档！**

## ❗ 致命错误避坑

### 文件位置错误导致编译失败
**这是UI设计师容易犯的文件组织错误！**

#### ❌ 错误：文件创建在错误位置
```
项目根目录/
├── Views/                              ← 错误位置！
│   ├── StatusTipView.axaml
│   ├── StatusTipView.axaml.cs
│   └── StatusBarView.axaml.cs
└── src/
    └── DotNetCampus.Terminal/
        └── Views/                      ← 正确位置
            ├── MainView.axaml
            └── MainView.axaml.cs
```

**典型编译错误**：
```
CSC : error AXN0001: Avalonia x:Name generator was unable to generate names for type 'DotNetCampus.Terminal.Views.StatusTipView'. 
The type 'DotNetCampus.Terminal.Views.StatusTipView' does not exist in the assembly.
```

#### ✅ 正确：文件必须在项目目录内
```
src/
└── DotNetCampus.Terminal/
    ├── Views/                          ← 正确位置
    │   ├── StatusTipView.axaml
    │   ├── StatusTipView.axaml.cs
    │   ├── StatusBarView.axaml
    │   └── StatusBarView.axaml.cs
    └── ViewModels/
        ├── StatusTipViewModel.cs
        └── StatusBarViewModel.cs
```

#### 错误原因分析
1. **AXAML文件声明了`x:Class`**：`x:Class="DotNetCampus.Terminal.Views.StatusTipView"`
2. **但C#文件不在项目中**：创建在了项目外部的错误位置
3. **Avalonia无法生成代码**：x:Name生成器找不到对应的类型

#### 修复方法
```powershell
# 删除错误位置的文件
Remove-Item "Views\StatusTipView.axaml.cs" -Force
Remove-Item "Views\StatusBarView.axaml" -Force
Remove-Item "Views\StatusBarView.axaml.cs" -Force
Remove-Item "Views" -Recurse -Force

# 确保文件在正确位置：src/DotNetCampus.Terminal/Views/
```

**记忆要点**：
- **AXAML和其C#代码隐藏文件必须在同一个项目中**
- **使用绝对路径创建文件时要特别小心路径是否正确**
- **遇到AXN0001错误时，首先检查文件位置是否正确**

### 字符级测量单位错误
**这是UI设计师最容易犯的致命错误！**

#### ❌ 错误的像素思维
```xml
<!-- 错误：这些数值在控制台中是字符单位！ -->
<StackPanel Spacing="10" Margin="10">          <!-- 10个字符间距！太大了！ -->
<Border Padding="10">                          <!-- 10个字符内边距！ -->
<TextBox Width="300">                          <!-- 300个字符宽度！ -->
<ProgressBar Height="8">                       <!-- 8个字符高度！ -->
<Button Padding="15,5">                        <!-- 15个字符宽，5个字符高！ -->
<TextBlock FontSize="16">                      <!-- 控制台中无意义！ -->
```

#### ✅ 正确的字符思维
```xml
<!-- 正确：以字符为单位思考 -->
<StackPanel Spacing="1" Margin="1">            <!-- 1个字符间距 -->
<Border Padding="1">                           <!-- 1个字符内边距 -->
<TextBox Width="40">                           <!-- 40个字符宽度 -->
<ProgressBar Height="1">                       <!-- 1个字符高度 -->
<Button Padding="2 0">                         <!-- 左右2个字符，上下0个字符 -->
<TextBlock>                                    <!-- 不设置FontSize -->
```

#### 字符级度量对照表
```
Spacing="1"   = 1个字符间距
Margin="1"    = 1个字符边距
Padding="1"   = 1个字符内边距
Width="20"    = 20个字符宽度
Height="1"    = 1个字符高度
Margin="2 0"  = 左右2个字符，上下0个字符
Padding="1 0" = 左右1个字符，上下0个字符
```

#### 常用尺寸参考
```xml
<!-- 小组件 -->
<Button Padding="1 0">短按钮</Button>
<Button Padding="2 0">中等按钮</Button>

<!-- 输入框 -->
<TextBox Width="20">短输入框</TextBox>
<TextBox Width="40">中等输入框</TextBox>
<TextBox Width="60">长输入框</TextBox>

<!-- 布局间距 -->
<StackPanel Spacing="0">紧密布局</StackPanel>
<StackPanel Spacing="1">标准布局</StackPanel>

<!-- 边框内边距 -->
<Border Padding="1">标准内边距</Border>
<Border Padding="2">宽松内边距</Border>
```

**记住：在Consolonia中，每个数字都是字符数量，不是像素！**

## 核心技术栈速查

### ViewModel基类选择
- `BindableRecord` - 基础属性绑定，使用 `SetField(ref field, value)`
- `TrackableBindableRecord` - 带变更跟踪，使用 `SetFieldTrackingChanges(ref field, value)`

### 命令类型速查
- `ActionCommand` - 同步无参数命令，适用于简单操作
- `AsyncCommand` - 异步命令，适用于I/O操作
- `InteractiveCommand` - 交互式命令，用于需要用户确认的操作

### Consolonia特殊语法
```xml
<!-- 文件扩展名 -->
*.axaml (不是 .xaml)

<!-- 命名空间 -->
xmlns:console="https://github.com/jinek/consolonia"

<!-- Button阴影禁用 (在Style中设置，不要在单个Button上设置) -->
<UserControl.Styles>
    <Style Selector="Button">
        <Setter Property="console:ButtonExtensions.Shadow" Value="False" />
    </Style>
</UserControl.Styles>

<!-- 数据类型绑定 -->
x:DataType="vm:SomeViewModel"
```

### 集合绑定
- 使用 `AvaloniaList<T>` 替代 `ObservableCollection<T>`
- 大数据集使用 `VirtualizingStackPanel`

### 异步UI更新
```csharp
// 后台线程更新UI
Dispatcher.UIThread.InvokeAsync(() => { /* UI更新代码 */ });
```

## 项目架构规范

### 文件夹结构
```
ViewModels/
├── RemoteDevices/
│   └── Ssh/
│       ├── SshDeviceSyncViewModel.cs      // 同步相关
│       ├── SshDeviceCommandsViewModel.cs  // 命令相关
│       └── SshDeviceDeployViewModel.cs    // 部署相关

Views/
├── RemoteDevices/
│   └── Ssh/
│       ├── SshDeviceSyncView.axaml       // 对应同步ViewModel
│       ├── SshDeviceCommandsView.axaml   // 对应命令ViewModel
│       └── SshDeviceDeployView.axaml     // 对应部署ViewModel
```

### ViewModel组合模式
```csharp
public record SshRemoteDeviceInfoViewModel : RemoteDeviceInfoNode
{
    // 子ViewModels - 使用简洁属性名
    public SshDeviceSyncViewModel Sync { get; }
    public SshDeviceCommandsViewModel Commands { get; }
    public SshDeviceDeployViewModel Deploy { get; }
}
```

## 常见错误避坑

### ❌ 错误做法
- 使用 `class` 继承 `record` 类型
- 在Button上直接设置 `console:ButtonExtensions.Shadow`
- 使用 `SetProperty` 方法（不存在）
- 使用 `RelayCommand`（项目中不存在）
- 忘记设置 `x:DataType`

### ✅ 正确做法
- 使用 `record` 继承 `record` 类型
- 在Styles中统一设置Button样式
- 根据基类使用 `SetField` 或 `SetFieldTrackingChanges`
- 使用项目中的 `ActionCommand`、`AsyncCommand` 等
- 始终设置强类型绑定 `x:DataType`

## 性能优化要点
- 选择合适的绑定模式（OneTime/OneWay/TwoWay）
- 大列表使用 `VirtualizingStackPanel`
- 使用 `x:DataType` 实现编译时绑定检查

## 相关知识库文档
- `Consolonia-Quick-Reference.md` - Consolonia核心语法
- `UI-Progress-And-Binding-Best-Practices.md` - 进度显示最佳实践
- `ViewModel-重构最佳实践.md` - ViewModel架构指南
- `Interactive-Command-Pattern-Guide.md` - 交互式命令模式

## 命令框架使用经验

### 静态CanExecute模式
项目框架的 `ActionCommand` 和 `AsyncCommand` 不支持动态刷新 CanExecute，需要手动管理：

```csharp
public SshDeviceDeployViewModel()
{
    // 初始化命令时不传递CanExecute委托
    DeployKeyCommand = new AsyncCommand(DeployKeyAsync);
    
    // 手动设置初始状态
    UpdateCommandStates();
}

// 在属性变更时手动更新命令状态
public bool IsDeploying
{
    get => _isDeploying;
    private set
    {
        if (SetFieldTrackingChanges(ref _isDeploying, value))
        {
            UpdateCommandStates(); // 关键：属性变更时更新命令状态
        }
    }
}

private void UpdateCommandStates()
{
    DeployKeyCommand.CanExecute = !IsDeploying && ConfirmOperation;
    RetryDeployCommand.CanExecute = CanRetry;
    // ...
}
```

### UI安全设计模式
SSH密钥部署等安全敏感操作的UI设计要点：

1. **多步确认机制**：用户必须勾选"理解安全影响"
2. **清晰的操作说明**：详细列出将执行的步骤
3. **安全警告**：使用醒目颜色（Orange/Red）标识风险操作
4. **进度反馈**：实时显示当前执行步骤和进度百分比
5. **错误恢复**：提供重试和回滚功能
6. **状态可视化**：不同阶段显示不同的UI区域（进度/错误/成功）

### Consolonia布局最佳实践
```xml
<!-- 使用ScrollViewer包装长内容 -->
<ScrollViewer>
    <StackPanel Spacing="10" Margin="10">
        <!-- 使用Border分组相关内容 -->
        <Border BorderBrush="DimGray" BorderThickness="1" Padding="10">
            <!-- 分组内容 -->
        </Border>
    </StackPanel>
</ScrollViewer>

<!-- 安全警告使用橙色边框 -->
<Border BorderBrush="Orange" BorderThickness="1" Padding="10">
    <TextBlock Text="⚠ 安全须知" Foreground="Orange" />
</Border>
```

## 📋 最新重构经验 (2025-07-09)

### 全局功能键状态栏重构成功案例

#### 重构前问题
- 功能键代码直接嵌入在MainView.axaml中，难以维护
- 没有统一的命令绑定，功能键点击无实际响应
- 功能键设计不够完整，缺少常用的TUI功能

#### 重构后解决方案
1. **创建专用状态栏控件**：`StatusBarView.axaml` + `StatusBarViewModel.cs`
2. **完整的功能键设计**：F1-F10 覆盖所有常用TUI操作
3. **统一的命令绑定**：每个功能键都有对应的命令实现
4. **模块化设计**：状态栏可以独立维护和测试

#### TUI程序标准功能键设计
```xml
F1  - 帮助    (ShowHelpCommand)
F2  - 连接    (ConnectCommand) 
F3  - 同步    (StartSyncCommand)
F4  - 新建    (NewDeviceCommand)
F5  - 刷新    (RefreshCommand)
F6  - 保存    (SaveConfigCommand)
F7  - 终端    (OpenShellCommand)
F8  - 搜索    (ToggleSearchCommand)
F9  - 设置    (SettingsCommand)
F10 - 退出    (ExitCommand)
```

#### 关键技术要点
- **依赖注入集成**：StatusBarViewModel 需要注册到 Startup.cs
- **命令类型选择**：同步用 ActionCommand，异步用 AsyncCommand
- **日志标签规范**：使用 `[StatusBar]` 标签便于调试
- **循环引用避免**：StatusBarViewModel 不能直接引用 ViewModels 命名空间

#### 编译错误解决
```csharp
// ❌ 错误：循环引用
using DotNetCampus.Terminal.ViewModels;

// ✅ 正确：避免循环引用，使用 EnsureGet 扩展方法
using DotNetCampus.Terminal.Framework.DependencyInjection;
_mainViewModel = serviceProvider.EnsureGet<MainViewModel>();
```

#### 功能键响应状态管理
**重要发现**：功能键的可用状态应该根据当前选中的设备/界面动态变化
- F2(连接)、F3(同步)、F6(保存)、F7(终端) 需要选中设备时才可用
- F1(帮助)、F4(新建)、F5(刷新)、F8(搜索)、F9(设置)、F10(退出) 始终可用

**未来改进方向**：
1. 实现功能键的动态可用状态管理
2. 添加快捷键支持 (KeyBinding)
3. 功能键标签的多语言支持
4. 根据选中项类型显示不同的功能键组合

---
*最后更新：2025年7月9日*
*下次更新时，请基于实际踩坑经验补充内容*
