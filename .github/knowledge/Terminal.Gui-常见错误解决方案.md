# Terminal.Gui 常见错误和解决方案

## 编译错误类型

### 1. 命名空间相关错误

#### 错误: "找不到类型或命名空间名称"

**常见情况**:
```csharp
// 错误写法
using Terminal.Gui;
var key = Key.Enter;  // 编译错误

// 正确写法
using Terminal.Gui.Input;
var key = Key.Enter;  // 正常编译
```

**解决方案**: 使用具体的命名空间而不是根命名空间

### 2. 控件属性错误

#### 错误: "Dim"、"Pos" 类型找不到

**错误信息**: `The name 'Dim' does not exist in the current context`

**解决方案**:
```csharp
using Terminal.Gui.ViewBase;  // Dim 和 Pos 在这里
```

### 3. 泛型推断失败

#### 错误: ListView.SetSource 方法调用失败

**错误信息**: `Cannot infer type arguments for SetSource`

**解决方案**:
```csharp
// 确保集合类型明确
var items = new ObservableCollection<string>();  // 明确指定泛型类型
_listView.SetSource(items);
```

### 4. 事件处理错误

#### 错误: TextField 事件不存在

**常见错误**:
```csharp
_textField.Accept += ...;  // v2 中已废弃
```

**正确写法**:
```csharp
_textField.KeyDown += (sender, key) => { ... };
```

## 运行时错误

### 1. NullReferenceException

**常见原因**: 控件未正确初始化

**解决方案**: 使用 `[MemberNotNull]` 属性并确保在构造函数中初始化：

```csharp
[MemberNotNull(nameof(_menuBar))]
private void SetupMenuBar()
{
    _menuBar = new MenuBarv2([...]);
}
```

### 2. 布局问题

**问题**: 控件显示不正确或重叠

**解决方案**:
1. 检查 X, Y, Width, Height 设置
2. 确保父容器尺寸足够
3. 使用 Dim.Fill() 和 Pos.X 正确组合

## 调试策略

### 1. 逐步验证

1. 先创建最简单的界面
2. 逐个添加控件
3. 每次添加后测试编译和运行

### 2. 使用 MessageBox 调试

```csharp
MessageBox.Query("调试", $"控件状态: {control.Visible}", "确定");
```

### 3. 检查继承关系

确保视图类正确继承：
```csharp
public partial class RootView : Toplevel  // 不是 View
```

## 最佳实践

### 1. 渐进式开发

1. 先建立基本框架
2. 添加简单控件
3. 实现事件处理
4. 优化布局和交互

### 2. 错误处理

```csharp
try
{
    // Terminal.Gui 操作
}
catch (Exception ex)
{
    MessageBox.ErrorQuery("错误", ex.Message, "确定");
}
```

### 3. 资源管理

```csharp
public class RootView : Toplevel, IDisposable
{
    public void Dispose()
    {
        // 清理资源
    }
}
```

## 求助时机

当遇到以下情况时，建议寻求人类开发者帮助：

1. **复杂的编译错误**: 涉及多个命名空间冲突
2. **API 版本问题**: v1 到 v2 的迁移问题
3. **性能问题**: 界面响应缓慢或内存泄漏
4. **平台兼容性**: 跨平台显示问题

这样可以避免 AI 在错误的方向上浪费时间，更快地解决问题。
