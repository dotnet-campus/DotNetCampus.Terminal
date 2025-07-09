# UI界面设计师经验总结

## 必读提醒
🔥 **在开始任何UI相关任务前，必须先阅读本文档！**

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

---
*最后更新：2025年7月9日*
*下次更新时，请基于实际踩坑经验补充内容*
