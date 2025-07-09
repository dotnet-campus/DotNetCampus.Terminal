# Consolonia UI 设计模式与最佳实践

## TUI 程序特殊性

### 1. 字符级测量单位
- 所有尺寸和间距都以字符为单位
- `Width="10"` = 10个字符宽度
- `Padding="1 0"` = 左右各1个字符的空间
- `Margin="0 1"` = 上下各1个字符的间距

### 2. 架构考虑
- **MainWindow**: 保留用于可能的GUI迁移，实际TUI运行时不显示
- **MainView**: 真正的TUI主界面，包含所有实际功能
- **Node类型**: 专门为TreeView设计的ViewModel，不是数据模型

### 3. 控制台美学
- 优先使用系统预定义颜色
- 保持简洁的视觉层次
- 避免过多装饰，注重信息密度

## 项目中的UI设计模式

### 1. 整体架构模式

#### 应用程序层次结构
```
App (Application)
├── MainWindow (Window) - 保留用于可能的GUI迁移
│   └── MainView (UserControl) - 主要的TUI界面
│       ├── TabControl
│       │   ├── 设备 Tab
│       │   └── 外壳 Tab
│       └── StatusBar
└── Other Views (UserControl)
    ├── CreateNewRemoteDeviceView
    ├── SshRemoteDeviceInfoView
    └── RemoteDeviceGroupView
```

**注意**: 在TUI模式下，`MainView` 是真正的主界面，`MainWindow` 仅用于GUI兼容性。

#### 数据流模式
```
ConfigurationManager → MainViewModel → UI Components
                                    ↓
                             RemoteDeviceNodes → TreeView
                                    ↓
                             DataTemplates → Specific Views
```

**注意**: RemoteDeviceNodes 是专门为 TreeView 设计的 ViewModel，不是传统的数据模型。

### 2. 布局设计模式

#### 主-详细页模式 (Master-Detail)
```xml
<Grid ColumnDefinitions="*,2*">
    <!-- 左侧：设备列表 (Master) -->
    <TreeView Grid.Column="0" ItemsSource="{Binding RemoteDevices}" />
    
    <!-- 右侧：详细信息 (Detail) -->
    <ContentControl Grid.Column="1" Content="{Binding SelectedItem}" />
</Grid>
```

#### 状态栏模式
```xml
<Grid RowDefinitions="*,Auto">
    <!-- 主要内容区域 -->
    <TabControl Grid.Row="0" />
    
    <!-- 固定底部状态栏 -->
    <StackPanel Grid.Row="1" Orientation="Horizontal">
        <!-- 功能按钮 -->
    </StackPanel>
</Grid>
```

### 3. 数据展示模式

#### 分层树结构
```xml
<TreeView>
    <TreeView.DataTemplates>
        <DataTemplate x:DataType="vm:CreateNewRemoteDeviceNode">
            <!-- 创建新设备节点模板 -->
        </DataTemplate>
        <DataTemplate x:DataType="vm:FavoriteDeviceGroupNode">
            <!-- 收藏设备组模板 -->
        </DataTemplate>
        <DataTemplate x:DataType="vm:RemoteDeviceGroupNode">
            <!-- 设备组模板 -->
        </DataTemplate>
        <DataTemplate x:DataType="vm:SshRemoteDeviceInfoNode">
            <!-- SSH设备信息模板 -->
        </DataTemplate>
    </TreeView.DataTemplates>
</TreeView>
```

#### 状态指示器模式
```xml
<Grid ColumnDefinitions="Auto,*,Auto">
    <!-- 状态指示器 -->
    <TextBlock Grid.Column="0" Width="1"
               Background="{Binding ConnectionState, Converter={StaticResource StateToBackgroundConverter}}"
               Foreground="{Binding ConnectionState, Converter={StaticResource StateToForegroundConverter}}"
               Text="{Binding ConnectionState, Converter={StaticResource StateToStringConverter}}" />
    
    <!-- 主要内容 -->
    <TextBlock Grid.Column="1" Text="{Binding Name}" />
    
    <!-- 操作按钮 -->
    <ToggleButton Grid.Column="2" Classes="FavoriteIcon" />
</Grid>
```

## 样式设计系统

### 1. 主题系统

#### 全局主题配置
```xml
<Application RequestedThemeVariant="Dark">
    <Application.Styles>
        <console:TurboVisionDarkTheme />
    </Application.Styles>
</Application>
```

#### 颜色方案
```csharp
// 状态颜色
ConnectionState.Testing  → "DarkGray"
ConnectionState.Online   → "Green"
ConnectionState.Offline  → "DarkRed"
ConnectionState.Default  → "White"

// 界面颜色
Background: "Black"
Foreground: "White"
Border: "DimGray"
Accent: "Green"
Warning: "Orange"
```

### 2. 控件样式模式

#### 功能键样式
```xml
<Style Selector="TextBlock.Fn">
    <Setter Property="Background" Value="White" />
    <Setter Property="Foreground" Value="Black" />
    <Setter Property="Padding" Value="1 0" />
    <Setter Property="Margin" Value="0 0 1 0" />
</Style>
```

#### 按钮样式
```xml
<Style Selector="Button">
    <Setter Property="console:ButtonExtensions.Shadow" Value="False" />
    <Style Selector="^ /template/ Border#InternalBorder">
        <Setter Property="Margin" Value="0" />
    </Style>
</Style>
```

#### 树视图项样式
```xml
<Style Selector="TreeViewItem">
    <Setter Property="Padding" Value="0" />
    <Setter Property="IsExpanded" Value="True" />
    
    <!-- 隐藏展开/收缩按钮 -->
    <Style Selector="^ /template/ ToggleButton#PART_ExpandCollapseChevron">
        <Setter Property="IsVisible" Value="False" />
    </Style>
    
    <!-- 分层样式 -->
    <Style Selector="^[Level=0]">
        <Setter Property="Margin" Value="0 1 0 0" />
    </Style>
    <Style Selector="^[Level=1]">
        <Setter Property="Margin" Value="1 1 0 0" />
    </Style>
</Style>
```

### 3. 响应式设计

#### 选择器层次结构
```xml
<!-- 基础样式 -->
<Style Selector="ToggleButton.FavoriteIcon">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="IsVisible" Value="False" />
</Style>

<!-- 状态样式 -->
<Style Selector="ToggleButton.FavoriteIcon:checked">
    <Setter Property="IsVisible" Value="True" />
</Style>

<!-- 交互样式 -->
<Style Selector="^:selected ToggleButton.FavoriteIcon">
    <Setter Property="IsVisible" Value="True" />
</Style>
<Style Selector="^:pointerover ToggleButton.FavoriteIcon">
    <Setter Property="IsVisible" Value="True" />
</Style>
```

## 数据绑定模式

### 1. 单向数据流

#### 从配置到UI
```
ConfigurationManager.FetchRemoteDevicesAsync()
    ↓
MainViewModel.RemoteDevices
    ↓
TreeView.ItemsSource
    ↓
DataTemplate 选择
    ↓
具体的 View
```

#### 状态更新流程
```
RemoteDeviceInfoNode.TestConnectionAsync()
    ↓
ConnectionState 属性变更
    ↓
PropertyChanged 事件
    ↓
UI 绑定更新
    ↓
转换器应用
    ↓
视觉效果变化
```

### 2. 转换器模式

#### 泛型转换器基类
```csharp
public class ConnectionStateToObjectConverter<T> : IValueConverter where T : class
{
    public T? Testing { get; set; }
    public T? Online { get; set; }
    public T? Offline { get; set; }
    public T? Default { get; set; }
}
```

#### 特化转换器
```csharp
public class ConnectionStateToStringConverter : ConnectionStateToObjectConverter<string>;
public class ConnectionStateToBrushConverter : ConnectionStateToObjectConverter<IBrush>;
```

### 3. 命令模式

#### 异步命令
```csharp
public AsyncCommand ReloadDevicesCommand { get; }

private async Task OnReloadDevices()
{
    // 执行异步操作
    var remoteDevices = await _configurationManager.FetchRemoteDevicesAsync();
    // 更新UI
}
```

## 性能优化模式

### 1. 虚拟化

#### 虚拟化面板
```xml
<TreeView.ItemsPanel>
    <ItemsPanelTemplate>
        <VirtualizingStackPanel Margin="0 1" />
    </ItemsPanelTemplate>
</TreeView.ItemsPanel>
```

### 2. 异步更新

#### 并发连接测试
```csharp
public async Task TestConnectionAsync()
{
    var tasks = devices.Select(async device =>
    {
        var result = await device.TestConnectionAsync();
        if (result)
        {
            Interlocked.Increment(ref _onlineCount);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                OnPropertyChanged(nameof(OnlineCount));
            });
        }
    });
    
    await Task.WhenAll(tasks);
}
```

### 3. 绑定优化

#### 绑定模式选择
```xml
<!-- 只读数据使用 OneTime -->
<TextBlock Text="{Binding Info.Name, Mode=OneTime}" />

<!-- 状态数据使用 OneWay -->
<TextBlock Text="{Binding OnlineCount, Mode=OneWay}" />

<!-- 用户输入使用 TwoWay -->
<TextBox Text="{Binding SearchText, Mode=TwoWay}" />
```

## 用户交互模式

### 1. 选择模式

#### 树视图选择
```xml
<TreeView x:Name="DeviceTreeView" ItemsSource="{Binding RemoteDevices}">
    <!-- 选择项自动绑定到 SelectedItem -->
</TreeView>

<ContentControl Content="{Binding #DeviceTreeView.SelectedItem}">
    <!-- 基于选择项显示不同内容 -->
</ContentControl>
```

### 2. 状态反馈

#### 收藏功能
```xml
<ToggleButton Classes="FavoriteIcon" IsCheckedChanged="FavoriteButton_IsCheckedChanged">
    <!-- 根据状态显示不同图标 -->
</ToggleButton>
```

#### 连接状态显示
```xml
<TextBlock Background="{Binding ConnectionState, Converter={StaticResource StateToBackgroundConverter}}"
           Foreground="{Binding ConnectionState, Converter={StaticResource StateToForegroundConverter}}"
           Text="{Binding ConnectionState, Converter={StaticResource StateToStringConverter}}" />
```

### 3. 功能键映射

#### 状态栏功能键
```xml
<StackPanel Orientation="Horizontal">
    <Button><TextBlock Text="F1" Classes="Fn" /><TextBlock Text="帮助" /></Button>
    <Button><TextBlock Text="F2" Classes="Fn" /><TextBlock Text="连接" /></Button>
    <Button><TextBlock Text="F3" Classes="Fn" /><TextBlock Text="同步" /></Button>
    <Button><TextBlock Text="F10" Classes="Fn" /><TextBlock Text="退出" /></Button>
</StackPanel>
```

## 可维护性模式

### 1. 视图分离

#### 每个功能独立视图
```
Views/
├── MainView.axaml                   # 主视图
├── CreateNewRemoteDeviceView.axaml  # 创建设备视图
├── SshRemoteDeviceInfoView.axaml    # SSH设备信息视图
└── RemoteDeviceGroupView.axaml      # 设备组视图
```

### 2. 样式复用

#### 全局样式定义
```xml
<UserControl.Styles>
    <!-- 可复用的样式 -->
    <Style Selector="Button">
        <!-- 通用按钮样式 -->
    </Style>
    <Style Selector="#StatusBarPanel>Button">
        <!-- 状态栏按钮特化样式 -->
    </Style>
</UserControl.Styles>
```

### 3. 数据模型分层

#### 接口抽象
```csharp
public interface IRemoteDeviceNode
{
    IReadOnlyList<IRemoteDeviceNode> Children { get; }
}
```

#### 具体实现
```csharp
public record CreateNewRemoteDeviceNode : IRemoteDeviceNode
public record FavoriteDeviceGroupNode : IRemoteDeviceNode
public record RemoteDeviceGroupNode : IRemoteDeviceNode
public record SshRemoteDeviceInfoNode : IRemoteDeviceNode
```

## 开发建议

### 1. 新功能开发流程

1. **定义数据模型**: 创建 ViewModel 和 Model
2. **设计视图结构**: 创建 XAML 布局
3. **实现数据绑定**: 连接 ViewModel 和 View
4. **添加样式**: 定义外观和交互效果
5. **优化性能**: 使用虚拟化和异步操作
6. **测试交互**: 验证用户体验

### 2. 调试技巧

#### 设计时数据
```xml
<Design.DataContext>
    <vm:MainViewModel />
</Design.DataContext>
```

#### 属性检查
```xml
<UserControl d:DesignWidth="800" d:DesignHeight="450">
```

### 3. 常见陷阱

- **忘记设置 DataContext**: 检查数据绑定是否正确
- **样式选择器错误**: 验证选择器语法
- **线程问题**: 确保UI更新在UI线程
- **性能问题**: 避免过度绑定和频繁更新

### 4. 扩展性考虑

- **新设备类型**: 通过 IRemoteDeviceNode 接口扩展
- **新视图**: 通过 DataTemplate 添加
- **新样式**: 通过样式选择器定制
- **新功能**: 通过 ViewModel 扩展

这个设计模式文档为后续的 UI 界面设计师提供了完整的参考框架，涵盖了从基础概念到高级模式的所有内容。
