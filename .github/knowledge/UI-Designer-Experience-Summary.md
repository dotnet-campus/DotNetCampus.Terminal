# UI界面设计师经验总结

## 必读提醒
🔥 **在开始任何UI相关任务前，必须先阅读本文档！**

## ❗ 致命错误避坑

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

---
*最后更新：2025年7月9日*
*下次更新时，请基于实际踩坑经验补充内容*
