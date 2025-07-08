# Consolonia 快速参考指南

## 测量单位

**重要**: TUI 程序中的所有单位都是字符级的：
- `Width="10"` = 10个字符宽度
- `Padding="1 0"` = 左右各1个字符
- `Margin="0 1"` = 上下各1个字符

**核心原则**: 像素 = 字符，每个像素对应一个控制台字符

## 基本设置

### 1. 项目配置
```xml
<!-- DotNetCampus.Terminal.csproj -->
<PackageReference Include="Consolonia" />
```

### 2. 程序入口
```csharp
// Program.cs
AppBuilder.Configure<App>()
    .UseConsolonia()
    .UseAutoDetectedConsole()
    .UseAutoDetectConsoleColorMode()
    .UseContainerServices()
    .StartWithConsoleLifetime(args);
```

### 3. 数据节点接口
```csharp
public interface IRemoteDeviceNode
{
    IReadOnlyList<IRemoteDeviceNode> Children { get; }
}
```

**注意**: 这些 Node 类本质上是专门用于 TreeView 的 ViewModel，不是传统意义上的数据模型。

### 4. 应用程序配置
```xml
<!-- App.axaml -->
<Application xmlns="https://github.com/avaloniaui"
             xmlns:console="https://github.com/jinek/consolonia"
             RequestedThemeVariant="Dark">
    <Application.Styles>
        <console:TurboVisionDarkTheme />
    </Application.Styles>
</Application>
```

## 常用控件

### 1. 基础布局
```xml
<!-- Grid 布局 -->
<Grid RowDefinitions="*,Auto" ColumnDefinitions="*,2*">
    <TreeView Grid.Row="0" Grid.Column="0" />
    <ContentControl Grid.Row="0" Grid.Column="1" />
    <StackPanel Grid.Row="1" Grid.ColumnSpan="2" Orientation="Horizontal" />
</Grid>
```

### 2. 按钮样式
```xml
<Button console:ButtonExtensions.Shadow="False">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="F1" Classes="Fn" />
        <TextBlock Text="帮助" />
    </StackPanel>
</Button>
```

### 3. 边框样式
```xml
<Border BorderThickness="1">
    <Border.BorderBrush>
        <console:LineBrush LineStyle="EdgeWide" Brush="DimGray" />
    </Border.BorderBrush>
</Border>
```

### 4. 树形控件
```xml
<TreeView ItemsSource="{Binding Items}">
    <TreeView.ItemTemplate>
        <TreeDataTemplate ItemsSource="{Binding Children}">
            <ContentControl Content="{Binding}" />
        </TreeDataTemplate>
    </TreeView.ItemTemplate>
    <TreeView.DataTemplates>
        <DataTemplate x:DataType="vm:DeviceNode">
            <TextBlock Text="{Binding Name}" />
        </DataTemplate>
    </TreeView.DataTemplates>
</TreeView>
```

## 数据绑定

### 1. 绑定模式
```xml
<!-- 一次性绑定 -->
<TextBlock Text="{Binding Name, Mode=OneTime}" />

<!-- 单向绑定 -->
<TextBlock Text="{Binding Status, Mode=OneWay}" />

<!-- 双向绑定 -->
<TextBox Text="{Binding Input, Mode=TwoWay}" />
```

### 2. 转换器
```xml
<!-- 定义转换器 -->
<UserControl.Resources>
    <converters:StateToBrushConverter x:Key="StateToBrushConverter"
                                     Online="Green"
                                     Offline="Red"
                                     Default="Gray" />
</UserControl.Resources>

<!-- 使用转换器 -->
<TextBlock Background="{Binding State, Converter={StaticResource StateToBrushConverter}}" />
```

### 3. 数据模板
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

## 样式系统

### 1. 选择器语法
```xml
<!-- 类型选择器 -->
<Style Selector="Button">
    <Setter Property="Background" Value="Black" />
</Style>

<!-- 类选择器 -->
<Style Selector="TextBlock.Fn">
    <Setter Property="Background" Value="White" />
</Style>

<!-- 模板选择器 -->
<Style Selector="Button ^ /template/ Border#InternalBorder">
    <Setter Property="Margin" Value="0" />
</Style>

<!-- 伪类选择器 -->
<Style Selector="Button:pointerover">
    <Setter Property="Background" Value="DarkGray" />
</Style>
```

### 2. 常用颜色
```xml
<!-- 背景色 -->
Background="Black"
Background="DimGray"
Background="Transparent"

<!-- 前景色 -->
Foreground="White"
Foreground="DarkGray"
Foreground="Green"
Foreground="Red"
Foreground="Orange"
```

## MVVM 模式

### 1. ViewModel 基类
```csharp
public record DeviceViewModel : BindableRecord
{
    private string _name = string.Empty;
    private ConnectionState _state;
    
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }
    
    public ConnectionState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }
}
```

### 2. 集合属性
```csharp
// 使用 AvaloniaList 而不是 ObservableCollection
public AvaloniaList<DeviceNode> Devices { get; } = [];
```

### 3. 异步命令
```csharp
public AsyncCommand RefreshCommand { get; }

public MainViewModel()
{
    RefreshCommand = new AsyncCommand(OnRefreshAsync);
}

private async Task OnRefreshAsync()
{
    // 异步操作
}
```

## 性能优化

### 1. 虚拟化
```xml
<TreeView.ItemsPanel>
    <ItemsPanelTemplate>
        <VirtualizingStackPanel />
    </ItemsPanelTemplate>
</TreeView.ItemsPanel>
```

### 2. 异步UI更新
```csharp
// 确保UI更新在UI线程
await Dispatcher.UIThread.InvokeAsync(() =>
{
    Status = "已连接";
});
```

### 3. 绑定优化
```xml
<!-- 避免不必要的双向绑定 -->
<TextBlock Text="{Binding ReadOnlyProperty, Mode=OneTime}" />

<!-- 使用 OneWay 而不是默认的 TwoWay -->
<TextBlock Text="{Binding DisplayProperty, Mode=OneWay}" />
```

## 常见问题

### 1. 控件不显示
- 检查 DataTemplate 的 x:DataType 是否匹配
- 确认 DataContext 是否正确设置
- 验证 ItemsSource 绑定是否正确

### 2. 样式不生效
- 检查选择器语法是否正确
- 确认 console: 命名空间是否正确引用
- 验证样式定义的位置和作用域

### 3. 数据绑定不工作
- 确保 ViewModel 继承自 BindableRecord
- 检查属性名称是否正确
- 验证绑定模式是否合适

### 4. 布局问题
- 检查 Grid 的行列定义
- 确认控件的 Grid.Row 和 Grid.Column 属性
- 验证 Margin 和 Padding 设置

## 开发工具

### 1. 文件扩展名
- XAML 文件使用 `.axaml` 扩展名
- 代码隐藏使用 `.axaml.cs` 扩展名

### 2. 设计时支持
```xml
<Design.DataContext>
    <vm:DeviceViewModel />
</Design.DataContext>
```

### 3. 调试技巧
```csharp
// 在代码隐藏中添加调试输出
System.Diagnostics.Debug.WriteLine($"DataContext: {DataContext?.GetType().Name}");
```

## 项目特定约定

### 1. 命名空间
```xml
xmlns:vm="using:DotNetCampus.Terminal.ViewModels"
xmlns:views="clr-namespace:DotNetCampus.Terminal.Views"
xmlns:converters="clr-namespace:DotNetCampus.Terminal.Views.Converters"
```

### 2. 数据节点接口
```csharp
public interface IRemoteDeviceNode
{
    IReadOnlyList<IRemoteDeviceNode> Children { get; }
}
```

### 3. 状态枚举
```csharp
public enum ConnectionState
{
    Default,
    Testing,
    Online,
    Offline
}
```

### 4. 主题配置
```xml
<!-- App.axaml 支持多种主题 -->
<Application.Styles>
    <console:TurboVisionDarkTheme />     <!-- 深色主题（推荐） -->
    <!-- <console:TurboVisionLightTheme /> --> <!-- 浅色主题 -->
    <!-- <console:MaterialTheme /> -->         <!-- Material 风格 -->
    <!-- <console:FluentTheme /> -->           <!-- Fluent 风格 -->
</Application.Styles>
```

## 绘制系统

### 1. LineBrush 边框样式
```xml
<!-- 单线边框 -->
<Border BorderThickness="1">
    <Border.BorderBrush>
        <console:LineBrush LineStyle="SingleLine" Brush="Gray" />
    </Border.BorderBrush>
</Border>

<!-- 双线边框 -->
<Border BorderThickness="1">
    <Border.BorderBrush>
        <console:LineBrush LineStyle="DoubleLine" Brush="Yellow" />
    </Border.BorderBrush>
</Border>

<!-- 混合边框样式 -->
<Border BorderThickness="1">
    <Border.BorderBrush>
        <console:LineBrush LineStyle="SingleLine DoubleLine SingleLine DoubleLine" Brush="DarkGreen" />
    </Border.BorderBrush>
</Border>
```

### 2. 线条和矩形
```xml
<!-- 水平线 -->
<Line StartPoint="1,0" EndPoint="10,0" Stroke="Yellow" StrokeThickness="1" />

<!-- 矩形 -->
<Rectangle Width="10" Height="10" Fill="Yellow" Stroke="Red" StrokeThickness="1"/>
```

### 3. 字体样式
```xml
<!-- 支持的字体属性 -->
<TextBlock Text="Hello World" 
           Background="Black"
           Foreground="Yellow" 
           FontWeight="Bold"
           TextDecorations="Underline" 
           FontStyle="Italic"/>
```

## 进度条和数据绑定

### 1. 进度条基本用法
```xml
<ProgressBar Value="{Binding Progress}" 
             Minimum="0" Maximum="100"
             Width="30" Height="1"
             Background="DimGray" 
             Foreground="Green" />
```

### 2. 动态显示隐藏
```xml
<StackPanel IsVisible="{Binding IsOperationRunning}">
    <TextBlock Text="进度:" Width="8" />
    <ProgressBar Value="{Binding Progress}" Width="30" Height="1" />
    <TextBlock Text="{Binding Progress, StringFormat={}{0:F0}%}" Width="4" />
</StackPanel>
```

### 3. 只读属性绑定问题
```csharp
// 当只读属性依赖其他属性时，需要手动触发更新通知
public SyncGroupStatus Status
{
    get => _status;
    set
    {
        if (SetField(ref _status, value))
        {
            // 手动触发只读属性的更新通知
            OnPropertyChanged(nameof(StatusSymbol));
            OnPropertyChanged(nameof(StatusColor));
        }
    }
}

public string StatusSymbol => Status switch
{
    SyncGroupStatus.Normal => "✓",
    SyncGroupStatus.Error => "⚠",
    _ => "○"
};
```

### 4. 进度报告模式
```csharp
var progress = new Progress<SyncProgress>(p =>
{
    GlobalProgress = p.TotalProgress;
    CurrentItemProgress = p.CurrentProgress;
});

IsOperationRunning = true;
try
{
    await longRunningOperation(progress);
}
finally
{
    IsOperationRunning = false;
}
```

这个快速参考指南涵盖了 Consolonia 开发的核心要点，便于快速查阅和使用。
