# DotNetCampus.Terminal Avalonia GUI 开发指南

## 项目UI现状分析

### 已实现的UI组件

#### 1. 应用程序框架
- ✅ **App.axaml**: 应用程序配置，使用 Fluent 主题系统
- ✅ **MainWindow.axaml**: 主窗口，支持现代桌面应用特性
- ✅ **MainView.axaml**: 主视图，包含完整的响应式布局结构

#### 2. 核心视图组件
- ✅ **设备管理界面**: 左侧设备树，右侧详细信息面板
- ✅ **TabControl**: 设备和外壳两个标签页，支持现代标签切换
- ✅ **TreeView**: 分层显示设备组和设备信息，支持展开/折叠
- ✅ **StatusBar**: 底部状态栏，显示连接状态和操作提示

#### 3. 专用视图
- ✅ **CreateNewRemoteDeviceView**: 新设备创建界面
- ✅ **SshRemoteDeviceInfoView**: SSH设备详细信息
- ✅ **RemoteDeviceGroupView**: 设备组视图

#### 4. 辅助组件
- ✅ **数据转换器**: 连接状态到颜色/字符串转换
- ✅ **样式系统**: 完整的控件样式定义
- ✅ **数据绑定**: MVVM模式实现

### 当前UI架构

```
MainWindow (现代桌面窗口，支持最大化/最小化/关闭)
└── MainView (主要内容区域)
    ├── TabControl (现代标签页控件)
    │   ├── 设备 Tab
    │   │   ├── 搜索框 (TextBox with 搜索图标)
    │   │   ├── 设备树 (TreeView with 现代展开图标)
    │   │   │   ├── 创建新设备节点
    │   │   │   ├── 收藏设备组
    │   │   │   └── 设备组 + 设备列表
    │   │   └── 详细信息面板 (ContentControl with Card 样式)
    │   └── 外壳 Tab (Terminal 集成面板)
    └── 状态栏 (现代状态显示)
```

## 待完善的UI功能

### 1. 搜索功能
```xml
<!-- 现代搜索框设计 -->
<Grid ColumnDefinitions="*,Auto">
    <TextBox Grid.Column="0" 
             Background="{DynamicResource TextControlBackground}"
             Watermark="搜索设备或组..." 
             Text="{Binding SearchText, Mode=TwoWay}" />
    <Button Grid.Column="1" Classes="icon" Command="{Binding SearchCommand}">
        <PathIcon Data="{StaticResource SearchIcon}" />
    </Button>
</Grid>

<!-- 需要实现：实时搜索过滤和高亮显示 -->
```

### 2. 外壳Tab内容
```xml
<!-- 现代终端集成设计 -->
<TabItem Header="外壳">
    <Grid RowDefinitions="Auto,*">
        <ToolBar Grid.Row="0">
            <Button Content="新建会话" Command="{Binding NewSessionCommand}" />
            <Button Content="分割窗口" Command="{Binding SplitPaneCommand}" />
            <Separator />
            <ComboBox ItemsSource="{Binding Profiles}" SelectedItem="{Binding SelectedProfile}" />
        </ToolBar>
        <Terminal:TerminalControl Grid.Row="1" Session="{Binding CurrentSession}" />
    </Grid>
</TabItem>

<!-- 需要实现：Terminal 控件集成和多会话管理 -->
```

### 3. 工具栏和按钮
```xml
<!-- 现代工具栏设计 -->
<ToolBar>
    <Button Classes="primary" Command="{Binding ConnectCommand}">
        <StackPanel Orientation="Horizontal">
            <PathIcon Data="{StaticResource ConnectIcon}" />
            <TextBlock Text="连接" Margin="8,0,0,0" />
        </StackPanel>
    </Button>
    <Button Command="{Binding DisconnectCommand}">
        <StackPanel Orientation="Horizontal">
            <PathIcon Data="{StaticResource DisconnectIcon}" />
            <TextBlock Text="断开" Margin="8,0,0,0" />
        </StackPanel>
    </Button>
    <Separator />
    <Button Command="{Binding RefreshCommand}">
        <PathIcon Data="{StaticResource RefreshIcon}" />
    </Button>
</ToolBar>

<!-- 需要实现：完整的命令绑定和图标资源 -->
```

### 4. 收藏功能
```csharp
// 当前状态：代码被注释
private void FavoriteButton_IsCheckedChanged(object? sender, RoutedEventArgs e)
{
    // 注释的代码...
}

// 需要实现：完整的收藏逻辑
```

## UI开发规范和约定

### 1. 文件命名约定
- **视图文件**: `{功能名}View.axaml`
- **代码隐藏**: `{功能名}View.axaml.cs`
- **视图模型**: `{功能名}ViewModel.cs`
- **树节点模型**: `{功能名}Node.cs`（用于TreeView的ViewModel）

### 2. 命名空间约定
```xml
xmlns="https://github.com/avaloniaui"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
xmlns:vm="using:DotNetCampus.Terminal.ViewModels"
xmlns:views="using:DotNetCampus.Terminal.Views"
mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
```

### 3. 数据绑定约定
```xml
<!-- 只读数据使用 OneTime -->
<TextBlock Text="{Binding Info.Name, Mode=OneTime}" />

<!-- 状态数据使用 OneWay -->
<TextBlock Text="{Binding OnlineCount, Mode=OneWay}" />

<!-- 用户输入使用 TwoWay -->
<TextBox Text="{Binding SearchText, Mode=TwoWay}" />

<!-- 强类型绑定（推荐） -->
<UserControl x:DataType="vm:MainViewModel">
    <TextBlock Text="{Binding DeviceName}" />
</UserControl>
```

### 4. 样式定义约定
```xml
<!-- 全局样式在 UserControl.Styles 中定义 -->
<UserControl.Styles>
    <Style Selector="Button">
        <Setter Property="Background" Value="{DynamicResource ButtonBackground}" />
        <Setter Property="CornerRadius" Value="4" />
    </Style>
    <Style Selector="Button.primary">
        <Setter Property="Background" Value="{DynamicResource AccentButtonBackground}" />
        <Setter Property="Foreground" Value="{DynamicResource AccentButtonForeground}" />
    </Style>
    <Style Selector="Button:pointerover">
        <Setter Property="Background" Value="{DynamicResource ButtonBackgroundPointerOver}" />
    </Style>
</UserControl.Styles>
```

## 关键UI设计原则

### 1. 现代桌面应用美学
- **主题系统**: 使用 Fluent 设计语言，支持浅色/深色主题切换
- **颜色方案**: 遵循系统主题，使用动态资源 `{DynamicResource}`
- **阴影和深度**: 利用现代UI的层次感，适当使用阴影效果
- **圆角设计**: 使用 `CornerRadius` 创建现代化的圆角界面元素

### 2. 响应式布局
- **自适应尺寸**: 支持窗口大小调整和不同屏幕分辨率
- **网格布局**: 使用 Grid 实现复杂的响应式布局
- **弹性容器**: 结合 StackPanel 和 DockPanel 实现灵活布局

### 3. 交互体验
- **平滑动画**: 使用 Avalonia 动画系统增强用户体验
- **视觉反馈**: 悬停、按下、选中等状态的明确视觉反馈
- **快捷键支持**: 完整的键盘导航和快捷键系统

### 4. 可访问性和国际化
- **多语言支持**: 使用资源文件实现界面国际化
- **高对比度**: 支持高对比度模式和无障碍访问
- **可缩放**: 支持 DPI 缩放和字体大小调整

## 开发工作流

### 1. 新功能开发步骤

#### 步骤1: 设计数据模型
```csharp
// 1. 定义接口
public interface INewFeatureNode : IRemoteDeviceNode
{
    // 新功能特有属性
}

// 2. 实现具体类（注意：这是TreeView的ViewModel，不是数据模型）
public record NewFeatureNode : BindableRecord, INewFeatureNode
{
    // 实现细节
}
```

#### 步骤2: 创建视图模型
```csharp
public class NewFeatureViewModel : BindableRecord
{
    private readonly IServiceProvider _serviceProvider;
    
    public NewFeatureViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    // 属性和命令
}
```

#### 步骤3: 设计XAML视图
```xml
<UserControl x:Class="DotNetCampus.Terminal.Views.NewFeatureView"
             xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="using:DotNetCampus.Terminal.ViewModels"
             mc:Ignorable="d" d:DesignWidth="400" d:DesignHeight="300"
             x:DataType="vm:NewFeatureViewModel">
    <Design.DataContext>
        <vm:NewFeatureViewModel />
    </Design.DataContext>
    
    <!-- 现代化视图内容 -->
    <Border Background="{DynamicResource CardBackground}" 
            BorderBrush="{DynamicResource CardBorderBrush}"
            BorderThickness="1" 
            CornerRadius="8" 
            Padding="16">
        <StackPanel Spacing="12">
            <TextBlock Text="示例标题" 
                       Classes="h3"
                       Foreground="{DynamicResource TextFillColorPrimary}" />
            <TextBlock Text="示例内容" 
                       Classes="body"
                       Foreground="{DynamicResource TextFillColorSecondary}" />
        </StackPanel>
    </Border>
</UserControl>
```

#### 步骤4: 实现代码隐藏
```csharp
public partial class NewFeatureView : UserControl
{
    public NewFeatureView()
    {
        InitializeComponent();
    }
    
    private NewFeatureViewModel ViewModel => (NewFeatureViewModel)DataContext!;
}
```

#### 步骤5: 集成到主界面
```xml
<!-- 在 MainView.axaml 中添加 DataTemplate -->
<DataTemplate x:DataType="vm:NewFeatureNode">
    <views:NewFeatureView />
</DataTemplate>
```

### 2. 样式开发步骤

#### 步骤1: 定义基础样式
```xml
<Style Selector="NewControl">
    <Setter Property="Background" Value="{DynamicResource ControlBackground}" />
    <Setter Property="Foreground" Value="{DynamicResource ControlForeground}" />
    <Setter Property="BorderBrush" Value="{DynamicResource ControlBorderBrush}" />
    <Setter Property="CornerRadius" Value="4" />
    <Setter Property="Padding" Value="8,4" />
</Style>
```

#### 步骤2: 添加状态样式
```xml
<Style Selector="NewControl:selected">
    <Setter Property="Background" Value="{DynamicResource AccentFillColorDefault}" />
    <Setter Property="Foreground" Value="{DynamicResource AccentTextFillColorPrimary}" />
</Style>
```

#### 步骤3: 定义交互样式
```xml
<Style Selector="NewControl:pointerover">
    <Setter Property="Background" Value="{DynamicResource ControlFillColorSecondary}" />
    <Setter Property="BorderBrush" Value="{DynamicResource ControlStrokeColorDefault}" />
</Style>

<Style Selector="NewControl:pressed">
    <Setter Property="Background" Value="{DynamicResource ControlFillColorTertiary}" />
</Style>
```

### 3. 调试和测试

#### 使用设计时数据
```xml
<Design.DataContext>
    <vm:NewFeatureViewModel />
</Design.DataContext>
```

#### 运行时调试
```csharp
// 在代码隐藏中添加调试代码
private void OnLoaded(object? sender, RoutedEventArgs e)
{
    System.Diagnostics.Debug.WriteLine($"View loaded: {DataContext?.GetType().Name}");
}
```

## 现有组件使用指南

### 1. 使用 TreeView 显示分层数据
```xml
<TreeView ItemsSource="{Binding Items}" 
          SelectedItem="{Binding SelectedItem, Mode=TwoWay}">
    <TreeView.ItemTemplate>
        <TreeDataTemplate ItemsSource="{Binding Children}" x:DataType="vm:ITreeNode">
            <Border Background="Transparent" 
                    CornerRadius="4" 
                    Padding="8,4">
                <ContentControl Content="{Binding}" />
            </Border>
        </TreeDataTemplate>
    </TreeView.ItemTemplate>
    <TreeView.DataTemplates>
        <!-- 为每种数据类型定义现代化模板 -->
        <DataTemplate x:DataType="vm:DeviceGroupNode">
            <StackPanel Orientation="Horizontal" Spacing="8">
                <PathIcon Data="{StaticResource FolderIcon}" 
                          Width="16" Height="16" />
                <TextBlock Text="{Binding Name}" 
                           VerticalAlignment="Center" />
            </StackPanel>
        </DataTemplate>
    </TreeView.DataTemplates>
</TreeView>
```

### 2. 使用 ContentControl 实现视图切换
```xml
<ContentControl Content="{Binding SelectedItem}">
    <ContentControl.ContentTransition>
        <CrossFade Duration="0:0:0.25" />
    </ContentControl.ContentTransition>
    <ContentControl.DataTemplates>
        <DataTemplate x:DataType="vm:SshDeviceViewModel">
            <views:SshDeviceView />
        </DataTemplate>
        <DataTemplate x:DataType="vm:DeviceGroupViewModel">
            <views:DeviceGroupView />
        </DataTemplate>
    </ContentControl.DataTemplates>
</ContentControl>
```

### 3. 使用转换器和主题资源
```xml
<UserControl.Resources>
    <converters:ConnectionStateToColorConverter x:Key="StateToColorConverter" />
    <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter" />
</UserControl.Resources>

<!-- 使用主题资源和转换器 -->
<Border Background="{Binding ConnectionState, Converter={StaticResource StateToColorConverter}}"
        CornerRadius="4" Padding="8,4">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <Ellipse Width="8" Height="8" 
                 Fill="{Binding IsConnected, Converter={StaticResource StateToColorConverter}}" />
        <TextBlock Text="{Binding DeviceName}" 
                   Foreground="{DynamicResource TextFillColorPrimary}" />
    </StackPanel>
</Border>
```

## 性能优化建议

### 1. 虚拟化大数据集
```xml
<TreeView.ItemsPanel>
    <ItemsPanelTemplate>
        <VirtualizingStackPanel />
    </ItemsPanelTemplate>
</TreeView.ItemsPanel>
```

### 2. 优化绑定性能
```xml
<!-- 使用强类型绑定提高性能 -->
<UserControl x:DataType="vm:DeviceViewModel">
    <!-- 避免不必要的双向绑定 -->
    <TextBlock Text="{Binding ReadOnlyProperty, Mode=OneTime}" />
    
    <!-- 使用 OneWay 而不是 TwoWay -->
    <TextBlock Text="{Binding DisplayProperty, Mode=OneWay}" />
    
    <!-- 合理使用 CompiledBinding -->
    <TextBlock Text="{CompiledBinding DeviceName}" />
</UserControl>
```

### 3. 异步操作
```csharp
// 避免阻塞UI线程
private async Task UpdateDataAsync()
{
    var data = await GetDataAsync();
    await Dispatcher.UIThread.InvokeAsync(() =>
    {
        // 更新UI
    });
}
```

## 常见问题解决

### 1. 数据绑定不工作
- 检查 DataContext 是否正确设置
- 验证属性名称是否正确
- 确认实现了 INotifyPropertyChanged

### 2. 样式不生效
- 检查选择器语法是否正确
- 确认样式定义的位置和优先级
- 验证目标控件是否匹配选择器

### 3. 布局问题
- 检查 Grid 的行列定义
- 确认控件的 Grid.Row 和 Grid.Column 设置
- 验证 Margin 和 Padding 设置

### 4. 性能问题
- 使用虚拟化面板
- 减少不必要的数据绑定
- 避免频繁的UI更新

## 待开发功能建议

### 1. 搜索过滤功能
```csharp
public class SearchableTreeViewModel : BindableRecord
{
    private string _searchText = string.Empty;
    
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
            {
                FilterItems();
            }
        }
    }
    
    private void FilterItems()
    {
        // 实现过滤逻辑
    }
}
```

### 2. 外壳Tab功能
```xml
<TabItem Header="外壳">
    <Grid RowDefinitions="Auto,*,Auto">
        <!-- 工具栏 -->
        <ToolBar Grid.Row="0">
            <Button Classes="icon" Command="{Binding NewTabCommand}" ToolTip.Tip="新建标签页">
                <PathIcon Data="{StaticResource AddIcon}" />
            </Button>
            <Button Classes="icon" Command="{Binding SplitPaneCommand}" ToolTip.Tip="分割面板">
                <PathIcon Data="{StaticResource SplitIcon}" />
            </Button>
            <Separator />
            <ComboBox ItemsSource="{Binding Profiles}" 
                      SelectedItem="{Binding SelectedProfile}"
                      MinWidth="150" />
        </ToolBar>
        
        <!-- 终端区域 -->
        <TabControl Grid.Row="1" ItemsSource="{Binding TerminalTabs}">
            <TabControl.ItemTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="{Binding Title}" />
                        <Button Classes="close" Command="{Binding CloseCommand}">
                            <PathIcon Data="{StaticResource CloseIcon}" Width="12" Height="12" />
                        </Button>
                    </StackPanel>
                </DataTemplate>
            </TabControl.ItemTemplate>
            <TabControl.ContentTemplate>
                <DataTemplate>
                    <Terminal:TerminalControl Session="{Binding Session}" />
                </DataTemplate>
            </TabControl.ContentTemplate>
        </TabControl>
        
        <!-- 状态栏 -->
        <StatusBar Grid.Row="2">
            <StatusBarItem>
                <TextBlock Text="{Binding CurrentDirectory}" />
            </StatusBarItem>
            <StatusBarItem HorizontalAlignment="Right">
                <TextBlock Text="{Binding ConnectionStatus}" />
            </StatusBarItem>
        </StatusBar>
    </Grid>
</TabItem>
```

### 3. 快捷键和手势
```csharp
private void MainView_KeyDown(object? sender, KeyEventArgs e)
{
    // 处理快捷键
    var modifiers = e.KeyModifiers;
    switch (e.Key)
    {
        case Key.F2:
            ConnectToDevice();
            e.Handled = true;
            break;
        case Key.F5:
            RefreshDeviceList();
            e.Handled = true;
            break;
        case Key.T when modifiers.HasFlag(KeyModifiers.Control):
            NewTerminalTab();
            e.Handled = true;
            break;
        case Key.W when modifiers.HasFlag(KeyModifiers.Control):
            CloseCurrentTab();
            e.Handled = true;
            break;
    }
}

// 处理鼠标手势
private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
{
    if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
    {
        // 显示上下文菜单
        ShowContextMenu(e.GetPosition(this));
    }
}
```

### 4. 上下文菜单和工具提示
```xml
<!-- 现代化上下文菜单 -->
<TreeView.ContextFlyout>
    <MenuFlyout>
        <MenuItem Header="连接" 
                  Command="{Binding ConnectCommand}"
                  InputGesture="F2">
            <MenuItem.Icon>
                <PathIcon Data="{StaticResource ConnectIcon}" />
            </MenuItem.Icon>
        </MenuItem>
        <MenuItem Header="编辑" 
                  Command="{Binding EditCommand}"
                  InputGesture="F4">
            <MenuItem.Icon>
                <PathIcon Data="{StaticResource EditIcon}" />
            </MenuItem.Icon>
        </MenuItem>
        <Separator />
        <MenuItem Header="删除" 
                  Command="{Binding DeleteCommand}"
                  InputGesture="Delete">
            <MenuItem.Icon>
                <PathIcon Data="{StaticResource DeleteIcon}" />
            </MenuItem.Icon>
        </MenuItem>
    </MenuFlyout>
</TreeView.ContextFlyout>

<!-- 工具提示 -->
<Button ToolTip.Tip="连接到远程设备" 
        ToolTip.ShowDelay="500">
    <PathIcon Data="{StaticResource ConnectIcon}" />
</Button>
```

这个开发指南为UI界面设计师提供了完整的项目背景和开发指导，包括现有代码的理解、开发规范、工作流程和常见问题的解决方案。
