# DotNetCampus.Terminal UI 开发指南

## 项目UI现状分析

### 已实现的UI组件

#### 1. 应用程序框架
- ✅ **App.axaml**: 应用程序配置，使用 TurboVisionDarkTheme
- ✅ **MainWindow.axaml**: 主窗口，包含缩放控制（保留用于可能的GUI迁移）
- ✅ **MainView.axaml**: 主视图，包含完整的布局结构

#### 2. 核心视图组件
- ✅ **设备管理界面**: 左侧设备树，右侧详细信息
- ✅ **TabControl**: 设备和外壳两个标签页
- ✅ **TreeView**: 分层显示设备组和设备信息
- ✅ **StatusBar**: 底部功能键栏（F1-F10）

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
MainWindow (缩放控制)
└── MainView (主要内容)
    ├── TabControl
    │   ├── 设备 Tab
    │   │   ├── 搜索框 (已定义但未实现)
    │   │   ├── 设备树 (TreeView)
    │   │   │   ├── 创建新设备节点
    │   │   │   ├── 收藏设备组
    │   │   │   └── 设备组 + 设备列表
    │   │   └── 详细信息面板 (ContentControl)
    │   └── 外壳 Tab (空)
    └── 状态栏 (功能键)
```

## 待完善的UI功能

### 1. 搜索功能
```xml
<!-- 当前状态：只有UI占位 -->
<TextBox Grid.Row="1" Background="Black" Padding="1 0" Watermark="搜索设备…" />

<!-- 需要实现：搜索逻辑和过滤功能 -->
```

### 2. 外壳Tab内容
```xml
<!-- 当前状态：空标签页 -->
<TabItem Header="外壳">
</TabItem>

<!-- 需要实现：Shell/Terminal相关功能 -->
```

### 3. 功能键响应
```xml
<!-- 当前状态：只有UI显示 -->
<Button>
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="F2" Classes="Fn" />
        <TextBlock Text="连接" />
    </StackPanel>
</Button>

<!-- 需要实现：实际的功能键响应 -->
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
xmlns:console="https://github.com/jinek/consolonia"
xmlns:vm="using:DotNetCampus.Terminal.ViewModels"
xmlns:views="using:DotNetCampus.Terminal.Views"
```

### 3. 数据绑定约定
```xml
<!-- 只读数据使用 OneTime -->
<TextBlock Text="{Binding Info.Name, Mode=OneTime}" />

<!-- 状态数据使用 OneWay -->
<TextBlock Text="{Binding OnlineCount, Mode=OneWay}" />

<!-- 用户输入使用 TwoWay -->
<TextBox Text="{Binding SearchText, Mode=TwoWay}" />
```

### 4. 样式定义约定
```xml
<!-- 全局样式在 UserControl.Styles 中定义 -->
<UserControl.Styles>
    <Style Selector="Button">
        <!-- 通用样式 -->
    </Style>
    <Style Selector="#SpecificId>Button">
        <!-- 特定区域样式 -->
    </Style>
</UserControl.Styles>
```

## 关键UI设计原则

### 1. 控制台美学
- **颜色方案**: 深色背景，浅色文字
- **边框样式**: 使用 `console:LineBrush` 创建控制台风格线条
- **按钮样式**: 禁用阴影 (`console:ButtonExtensions.Shadow="False"`)
- **间距控制**: 使用 `Padding="1 0"` 等字符级间距（TUI程序中1表示1个字符宽度）

### 2. 信息密度
- **状态指示**: 使用单字符和颜色表示状态
- **分层显示**: 通过缩进和边距体现层次
- **紧凑布局**: 最大化信息显示效率

### 3. 交互反馈
- **状态变化**: 连接状态的实时更新
- **选择反馈**: 选中项的视觉变化
- **悬停效果**: 鼠标悬停时的样式变化

### 4. 可访问性
- **键盘导航**: 支持Tab键导航
- **功能键**: 提供快捷键操作
- **状态提示**: 清晰的状态指示

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
             x:DataType="vm:NewFeatureViewModel">
    <Design.DataContext>
        <vm:NewFeatureViewModel />
    </Design.DataContext>
    
    <!-- 视图内容 - 记住所有尺寸都是字符级别的 -->
    <Grid>
        <TextBlock Text="示例" Padding="1 0" />  <!-- 左右各1个字符的填充 -->
    </Grid>
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
    <Setter Property="Background" Value="Black" />
    <Setter Property="Foreground" Value="White" />
</Style>
```

#### 步骤2: 添加状态样式
```xml
<Style Selector="NewControl:selected">
    <Setter Property="Background" Value="DarkBlue" />
</Style>
```

#### 步骤3: 定义交互样式
```xml
<Style Selector="NewControl:pointerover">
    <Setter Property="Background" Value="DarkGray" />
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
<TreeView ItemsSource="{Binding Items}">
    <TreeView.ItemTemplate>
        <TreeDataTemplate ItemsSource="{Binding Children}">
            <ContentControl Content="{Binding}" />
        </TreeDataTemplate>
    </TreeView.ItemTemplate>
    <TreeView.DataTemplates>
        <!-- 为每种数据类型定义模板 -->
    </TreeView.DataTemplates>
</TreeView>
```

### 2. 使用 ContentControl 实现视图切换
```xml
<ContentControl Content="{Binding SelectedItem}">
    <ContentControl.DataTemplates>
        <DataTemplate x:DataType="vm:TypeA">
            <views:ViewA />
        </DataTemplate>
        <DataTemplate x:DataType="vm:TypeB">
            <views:ViewB />
        </DataTemplate>
    </ContentControl.DataTemplates>
</ContentControl>
```

### 3. 使用转换器进行数据转换
```xml
<UserControl.Resources>
    <converters:StateToColorConverter x:Key="StateToColorConverter"
                                      Online="Green"
                                      Offline="Red"
                                      Default="Gray" />
</UserControl.Resources>

<TextBlock Background="{Binding State, Converter={StaticResource StateToColorConverter}}" />
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
<!-- 避免不必要的双向绑定 -->
<TextBlock Text="{Binding ReadOnlyProperty, Mode=OneTime}" />

<!-- 使用 OneWay 而不是 TwoWay -->
<TextBlock Text="{Binding DisplayProperty, Mode=OneWay}" />
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
    <Grid>
        <!-- 终端相关功能 -->
        <Terminal:TerminalControl />
    </Grid>
</TabItem>
```

### 3. 功能键响应
```csharp
private void MainView_KeyDown(object? sender, KeyEventArgs e)
{
    switch (e.Key)
    {
        case Key.F1:
            ShowHelp();
            break;
        case Key.F2:
            ConnectToDevice();
            break;
        // 其他功能键
    }
}
```

### 4. 上下文菜单
```xml
<TreeView.ContextMenu>
    <ContextMenu>
        <MenuItem Header="连接" Command="{Binding ConnectCommand}" />
        <MenuItem Header="编辑" Command="{Binding EditCommand}" />
        <MenuItem Header="删除" Command="{Binding DeleteCommand}" />
    </ContextMenu>
</TreeView.ContextMenu>
```

这个开发指南为UI界面设计师提供了完整的项目背景和开发指导，包括现有代码的理解、开发规范、工作流程和常见问题的解决方案。
