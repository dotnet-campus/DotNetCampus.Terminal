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

### Avalonia XAML 语法要点
**基于现代GUI框架的设计原则！**

#### ✅ 标准像素单位思维
```xml
<!-- 正确：Avalonia使用标准像素单位 -->
<StackPanel Spacing="10" Margin="15">          <!-- 10像素间距，15像素边距 -->
<Border Padding="12">                          <!-- 12像素内边距 -->
<TextBox Width="300" Height="32">              <!-- 300像素宽，32像素高 -->
<ProgressBar Height="6">                       <!-- 6像素高度 -->
<Button Padding="16,8">                        <!-- 16像素宽，8像素高 -->
<TextBlock FontSize="14">                      <!-- 14号字体 -->
```

#### 现代GUI尺寸参考表
```
Spacing="8"    = 紧密间距
Spacing="12"   = 标准间距  
Spacing="16"   = 宽松间距
Margin="8"     = 小边距
Margin="16"    = 标准边距
Margin="24"    = 大边距
Padding="8"    = 小内边距
Padding="12"   = 标准内边距
Padding="16"   = 大内边距
Height="32"    = 标准控件高度
Height="40"    = 大控件高度
FontSize="12"  = 小字体
FontSize="14"  = 标准字体
FontSize="16"  = 大字体
```

#### 常用尺寸参考
```xml
<!-- 按钮设计 -->
<Button Padding="16,8" MinHeight="32">标准按钮</Button>
<Button Padding="12,6" MinHeight="28">小按钮</Button>
<Button Padding="20,10" MinHeight="40">大按钮</Button>

<!-- 输入框 -->
<TextBox Width="200" Height="32">短输入框</TextBox>
<TextBox Width="300" Height="32">中等输入框</TextBox>
<TextBox Width="400" Height="32">长输入框</TextBox>

<!-- 布局间距 -->
<StackPanel Spacing="8">紧密布局</StackPanel>
<StackPanel Spacing="12">标准布局</StackPanel>
<StackPanel Spacing="16">宽松布局</StackPanel>

<!-- 边框内边距 -->
<Border Padding="12">标准内边距</Border>
<Border Padding="16">宽松内边距</Border>
```

**记住：Avalonia使用标准像素单位，可以精确控制界面美观度！**

## 核心技术栈速查

### ViewModel基类选择
- `BindableRecord` - 基础属性绑定，使用 `SetField(ref field, value)`
- `TrackableBindableRecord` - 带变更跟踪，使用 `SetFieldTrackingChanges(ref field, value)`

### 命令类型速查
- `ActionCommand` - 同步无参数命令，适用于简单操作
- `AsyncCommand` - 异步命令，适用于I/O操作
- `InteractiveCommand` - 交互式命令，用于需要用户确认的操作

### Avalonia XAML 核心语法
```xml
<!-- 文件扩展名：.axaml -->
*.axaml

<!-- Avalonia 标准命名空间 -->
xmlns="https://github.com/avaloniaui"

<!-- 使用 Fluent 主题 -->
<Application.Styles>
    <FluentTheme />
</Application.Styles>

<!-- 数据类型绑定 -->
x:DataType="vm:SomeViewModel"

<!-- 强类型资源引用 -->
<Button Background="{DynamicResource SystemAccentColor}" />
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
- 使用已弃用的 Consolonia 特有语法
- 使用 `SetProperty` 方法（不存在）
- 使用 `RelayCommand`（项目中不存在）
- 忘记设置 `x:DataType`

### ✅ 正确做法
- 使用 `record` 继承 `record` 类型
- 使用标准 Avalonia XAML 语法
- 根据基类使用 `SetField` 或 `SetFieldTrackingChanges`
- 使用项目中的 `ActionCommand`、`AsyncCommand` 等
- 始终设置强类型绑定 `x:DataType`

## Avalonia GUI 设计模式

### 现代布局容器
```xml
<!-- 网格布局 -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>
    <!-- 内容 -->
</Grid>

<!-- 停靠面板 -->
<DockPanel>
    <TextBlock DockPanel.Dock="Top" Text="标题" />
    <StatusBar DockPanel.Dock="Bottom" />
    <ContentPresenter />
</DockPanel>

<!-- 弹性包装面板 -->
<WrapPanel Orientation="Horizontal" ItemWidth="200" ItemHeight="100">
    <!-- 自动换行的项目 -->
</WrapPanel>
```

### 数据模板和样式
```xml
<!-- 数据模板 -->
<UserControl.Resources>
    <DataTemplate x:Key="DeviceTemplate" x:DataType="vm:DeviceViewModel">
        <Border Padding="8" Margin="4" Background="LightGray">
            <StackPanel>
                <TextBlock Text="{Binding Name}" FontWeight="Bold" />
                <TextBlock Text="{Binding Status}" />
            </StackPanel>
        </Border>
    </DataTemplate>
</UserControl.Resources>

<!-- 样式定义 -->
<UserControl.Styles>
    <Style Selector="Button.primary">
        <Setter Property="Background" Value="{DynamicResource SystemAccentColor}" />
        <Setter Property="Foreground" Value="White" />
        <Setter Property="Padding" Value="16,8" />
    </Style>
</UserControl.Styles>
```

### 现代颜色和主题
```xml
<!-- 使用系统颜色 -->
<Button Background="{DynamicResource SystemAccentColor}" />
<TextBlock Foreground="{DynamicResource SystemBaseHighColor}" />

<!-- 状态颜色 -->
<Border Background="{DynamicResource SystemFillColorSuccess}">成功</Border>
<Border Background="{DynamicResource SystemFillColorCaution}">警告</Border>
<Border Background="{DynamicResource SystemFillColorCritical}">错误</Border>
```

## 性能优化要点
- 选择合适的绑定模式（OneTime/OneWay/TwoWay）
- 大列表使用 `VirtualizingStackPanel`
- 使用 `x:DataType` 实现编译时绑定检查
- 使用硬件加速和GPU渲染
- 合理使用缓存和资源管理

## 相关知识库文档
- `Avalonia-快速参考指南.md` - Avalonia核心语法
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
SSH密钥部署等安全敏感操作的GUI设计要点：

1. **多步确认机制**：用户必须勾选"理解安全影响"
2. **清晰的操作说明**：详细列出将执行的步骤
3. **状态可视化**：使用图标和颜色表示不同状态
4. **进度反馈**：实时显示当前执行步骤和进度百分比
5. **错误恢复**：提供重试和回滚功能
6. **响应式设计**：适应不同窗口大小和分辨率

### Avalonia 现代界面设计
```xml
<!-- 使用卡片式设计 -->
<Border Background="{DynamicResource SystemChromeLowColor}" 
        CornerRadius="8" 
        Padding="16" 
        Margin="8">
    <StackPanel Spacing="12">
        <!-- 卡片内容 -->
    </StackPanel>
</Border>

<!-- 状态指示器 -->
<Grid>
    <Ellipse Width="12" Height="12" 
             Fill="{DynamicResource SystemFillColorSuccess}" 
             IsVisible="{Binding IsConnected}" />
    <Ellipse Width="12" Height="12" 
             Fill="{DynamicResource SystemFillColorCritical}" 
             IsVisible="{Binding !IsConnected}" />
</Grid>

<!-- 现代按钮组 -->
<StackPanel Orientation="Horizontal" Spacing="8">
    <Button Classes="primary" Command="{Binding ConnectCommand}">连接</Button>
    <Button Classes="secondary" Command="{Binding DisconnectCommand}">断开</Button>
    <Button Classes="danger" Command="{Binding DeleteCommand}">删除</Button>
</StackPanel>
```

## 📋 Avalonia GUI 迁移经验 (2025-07-17)

### 从 Consolonia TUI 到 Avalonia GUI 的关键变化

#### 界面设计理念转变
- **从字符界面到像素界面**：使用标准像素单位而不是字符单位
- **从单色到真彩色**：充分利用现代显示器的颜色表现能力
- **从键盘导航到鼠标交互**：增加鼠标悬停、拖拽等现代交互
- **从固定布局到响应式布局**：适应不同窗口大小和DPI缩放

#### 技术架构变化
1. **移除Consolonia特有语法**：
   - 删除 `xmlns:console="https://github.com/jinek/consolonia"`
   - 移除 `console:ButtonExtensions.Shadow="False"`
   - 清理字符级别的尺寸设置

2. **采用标准Avalonia特性**：
   - 使用 `xmlns="https://github.com/avaloniaui"`
   - 支持现代主题系统：FluentTheme
   - 利用硬件加速和GPU渲染

3. **保持业务逻辑不变**：
   - ViewModels 层完全复用
   - 命令绑定和数据绑定保持一致
   - 服务层和配置系统无需修改

#### 界面优化重点
- **现代化视觉设计**：使用卡片式布局、圆角边框、阴影效果
- **改进的状态反馈**：丰富的颜色状态、图标指示、动画过渡
- **更好的信息密度**：摆脱字符界面限制，可以显示更多设备信息
- **增强的交互体验**：鼠标悬停提示、右键菜单、拖拽操作

#### 关键技术要点
- **XAML语法升级**：从TUI约束的布局到完整的GUI布局系统
- **资源系统使用**：利用Avalonia的主题资源和样式系统
- **响应式设计**：支持窗口缩放和多分辨率适配
- **无障碍支持**：更好的屏幕阅读器支持和键盘导航

#### 兼容性保障
- **ViewModels层100%复用**：所有属性绑定和命令绑定保持不变
- **配置系统无变化**：JSON配置文件格式和存储位置不变
- **业务逻辑不受影响**：SSH连接、文件同步等核心功能保持一致

### 迁移检查清单
- [ ] ✅ 移除所有Consolonia命名空间引用
- [ ] ✅ 更新为标准Avalonia XAML语法
- [ ] ✅ 采用像素单位替代字符单位
- [ ] ✅ 使用现代主题和颜色系统
- [ ] ✅ 验证ViewModels绑定正常工作
- [ ] ✅ 测试所有命令和交互功能
- [ ] ✅ 确保响应式布局正常工作

---
*最后更新：2025年7月17日 - Avalonia GUI迁移版本*
*下次更新时，请基于Avalonia GUI的实际开发经验补充内容*
