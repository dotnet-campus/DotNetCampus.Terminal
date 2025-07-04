# Terminal.Gui 使用指南和最佳实践

## 概述

Terminal.Gui 是一个功能强大的 .NET 控制台 UI 框架，用于创建现代化的终端用户界面。本文档总结了在 DotNetCampus Terminal 项目中使用 Terminal.Gui 的经验和最佳实践。

## 基本架构

### 1. 核心概念

- **Application**: 应用程序的主入口点，管理应用程序的生命周期
- **Toplevel**: 顶级窗口容器，通常作为主窗口
- **View**: 所有 UI 控件的基类
- **Window**: 带边框和标题的窗口控件
- **Dialog**: 模态对话框

### 2. 命名空间结构

Terminal.Gui v2 使用了新的命名空间结构，需要正确引用：

```csharp
using Terminal.Gui.Views;      // 视图和控件
using Terminal.Gui.Input;      // 输入相关（Key, KeyEventArgs等）
using Terminal.Gui.App;        // 应用程序相关
using Terminal.Gui.ViewBase;   // 视图基类
```

**重要提示**: 不要使用 `using Terminal.Gui;` 这种全局引用，会导致命名冲突。

## 常用控件详解

### 1. MenuBarv2 (菜单栏)

```csharp
private void SetupMenuBar()
{
    _menuBar = new MenuBarv2([
        new MenuBarItemv2
        {
            Title = "_文件",  // 下划线表示快捷键
            PopoverMenu = new PopoverMenu([
                new MenuItemv2
                {
                    Title = "_新建",
                    Key = Key.N.WithCtrl,  // Ctrl+N
                    Action = HandleNew,    // 回调方法
                },
                new Line(),  // 分隔线
                // 更多菜单项...
            ])
        }
    ]);
    
    Add(_menuBar);  // 添加到容器
}
```

**要点**:
- 使用 `MenuBarv2` 而不是旧版的 `MenuBar`
- `Title` 中的下划线 `_` 表示快捷键字母
- 使用 `Key.X.WithCtrl` 设置组合键
- `Action` 属性接受无参数的委托

### 2. StatusBar (状态栏)

```csharp
_statusBar = new StatusBar([
    new Shortcut
    {
        Title = "_帮助",
        Key = Key.F1,
        Action = ShowHelp,
    }
]);
```

### 3. FrameView (框架视图)

```csharp
_frameView = new FrameView
{
    Title = "标题",
    X = 0,
    Y = 1,
    Width = Dim.Percent(50),    // 占用50%宽度
    Height = Dim.Fill(2),       // 填充高度，底部留2行
};
```

### 4. ListView (列表视图)

```csharp
_listView = new ListView
{
    X = 0,
    Y = 0,
    Width = Dim.Fill(),
    Height = Dim.Fill(),
};

// 设置数据源
var items = new ObservableCollection<string> { "项目1", "项目2" };
_listView.SetSource(items);
```

**注意**: 使用 `SetSource()` 方法而不是直接设置 `Source` 属性。

### 5. TextField (文本输入框)

```csharp
_textField = new TextField
{
    X = 0,
    Y = 0,
    Width = Dim.Fill(),
    Height = 1,
    Text = "默认文本",
};

// 处理按键事件
_textField.KeyDown += (sender, key) =>
{
    if (key == Key.Enter)
    {
        // 处理回车键
    }
};
```

**要点**:
- 使用 `KeyDown` 事件处理按键
- `Key.Enter` 表示回车键
- 避免使用已废弃的 `Accept` 事件

## 布局系统

### 1. 定位 (Pos)

```csharp
X = Pos.Center(),           // 居中
X = Pos.Right(otherView),   // 在其他视图右侧
Y = Pos.Bottom(otherView),  // 在其他视图下方
```

### 2. 尺寸 (Dim)

```csharp
Width = Dim.Fill(),         // 填充剩余宽度
Width = Dim.Percent(50),    // 占用50%宽度
Height = Dim.Fill(2),       // 填充高度，底部留2行
```

### 3. 布局最佳实践

1. **菜单栏**: Y = 0
2. **主内容区**: Y = 1 (菜单栏下方)
3. **状态栏**: 自动定位到底部
4. **命令输入框**: Y = Pos.Bottom(mainContent)

## 事件处理

### 1. 按键事件

```csharp
control.KeyDown += (sender, key) =>
{
    switch (key)
    {
        case Key.Enter:
            // 处理回车
            break;
        case Key.F5:
            // 处理F5
            break;
    }
};
```

### 2. 菜单事件

```csharp
new MenuItemv2
{
    Title = "菜单项",
    Action = () => HandleMenuAction(),  // Lambda表达式
    // 或
    Action = HandleMenuAction,          // 方法引用
}
```

## 常见问题和解决方案

### 1. 命名空间冲突

**问题**: 使用 `using Terminal.Gui;` 导致类型冲突

**解决**: 使用具体的命名空间
```csharp
using Terminal.Gui.Views;
using Terminal.Gui.Input;
// 避免: using Terminal.Gui;
```

### 2. SetSource 泛型推断失败

**问题**: `_listView.SetSource(items)` 编译错误

**解决**: 确保数据源类型匹配
```csharp
var items = new ObservableCollection<string>();  // 明确类型
_listView.SetSource(items);
```

### 3. Key 类型引用错误

**问题**: `Key.Enter` 找不到

**解决**: 正确引用命名空间
```csharp
using Terminal.Gui.Input;  // Key 在这个命名空间中
```

### 4. MemberNotNull 属性

对于在构造函数中初始化的字段，使用 `[MemberNotNull]` 属性：

```csharp
[MemberNotNull(nameof(_menuBar), nameof(_statusBar))]
private void InitializeComponent()
{
    // 初始化代码
}
```

## 应用程序生命周期

### 1. 基本结构

```csharp
// Program.cs
Application.Run<MainWindow>().Dispose();
Application.Shutdown();
```

### 2. 在主窗口中

```csharp
public class RootView : Toplevel  // 继承 Toplevel
{
    public RootView()
    {
        InitializeComponent();
        SetupLayout();
    }
}
```

## 性能优化建议

1. **避免频繁更新**: 批量更新 ListView 数据源
2. **及时释放资源**: 实现 IDisposable 接口
3. **使用异步操作**: 避免阻塞 UI 线程

## 调试技巧

1. **使用 MessageBox**: 快速显示调试信息
```csharp
MessageBox.Query("调试", $"值: {value}", "确定");
```

2. **检查控件层次**: 确保正确的父子关系
3. **验证布局**: 使用 Dim.Fill() 和 Pos.X 组合

## 版本兼容性

- 本项目使用 Terminal.Gui v2
- 避免使用 v1 的过时 API
- 优先使用新的控件（如 MenuBarv2）

## 扩展阅读

- [Terminal.Gui 官方文档](https://gui-cs.github.io/Terminal.Gui/)
- [API 参考](https://gui-cs.github.io/Terminal.Gui/api/Terminal.Gui.html)
- [示例项目 UICatalog](https://github.com/gui-cs/Terminal.Gui/tree/v2_develop/UICatalog)
