# DotNetCampus.Logger 使用指南

DotNetCampus.Logger 是一个高性能的 .NET 日志库，支持源生成器，提供零依赖的日志记录能力。

## 1. 核心特性

### 源生成器支持
- 使用源生成器生成日志代码，无运行时依赖
- 支持编译时条件编译（TRACE、DEBUG）
- 自动生成高性能的日志记录代码

### 静态方法调用
- 提供静态 `Log` 类，可在任何地方直接使用
- 无需依赖注入，简化使用流程
- 与传统依赖注入方式性能等价

### 多级别日志支持
- 支持 Trace、Debug、Info、Warn、Error、Fatal 六个级别
- 支持条件编译的调试日志
- 灵活的日志级别控制

## 2. 基础使用

### 安装
```xml
<PackageReference Include="DotNetCampus.Logger" />
```

### 初始化
```csharp
new LoggerBuilder()
    .WithLevel(LogLevel.Debug)
    .AddWriter(new ConsoleLogger())
    .Build()
    .IntoGlobalStaticLog();
```

### 基本日志记录
```csharp
// 基本日志级别（始终编译）
Log.Trace("这是追踪信息");
Log.Debug("这是调试信息");
Log.Info("这是一般信息");
Log.Warn("这是警告信息");
Log.Error("这是错误信息");
Log.Fatal("这是致命错误");

// 条件编译日志（仅在 DEBUG 模式下编译）
Log.DebugLogger.Info("调试模式下的信息");
Log.DebugLogger.Error("调试模式下的错误");

// 条件编译日志（仅在 TRACE 模式下编译）
Log.TraceLogger.Debug("追踪模式下的调试信息");
```

### 带标签的日志记录
```csharp
// 使用标签便于过滤和分类
Log.Info("[FileSync] 开始同步文件");
Log.Error("[Network] 连接超时");
Log.Debug("[UI] 用户点击了按钮");
```

## 3. 日志级别说明

### 级别定义
- **Trace**: 最详细的信息，通常仅在追踪问题时使用
- **Debug**: 调试信息，用于开发阶段问题诊断
- **Info**: 一般信息，记录程序正常运行的关键节点
- **Warn**: 警告信息，程序可以继续运行但存在潜在问题
- **Error**: 错误信息，程序遇到错误但可以恢复
- **Fatal**: 致命错误，程序无法继续运行

### 使用建议
```csharp
// Info - 记录重要的业务流程
Log.Info("[FileSync] 开始同步目录 ProjectA: /local/path -> /remote/path");

// Debug - 记录详细的执行步骤
Log.Debug("[FileSync] 正在上传文件: file.txt -> /remote/file.txt");

// Warn - 记录可恢复的问题
Log.Warn("[FileSync] 同步操作被用户取消");

// Error - 记录错误但程序可继续
Log.Error("[FileSync] 上传文件失败: file.txt. 错误: 权限不足");

// Fatal - 记录致命错误
Log.Fatal("[System] 系统内存不足，程序即将退出");
```

## 4. 高级特性

### 日志过滤
支持通过命令行参数过滤日志：
```bash
# 只显示包含 FileSync 标签的日志
--log-console-tags FileSync

# 显示包含 FileSync 或 Network 标签的日志
--log-console-tags FileSync,Network

# 必须同时包含 FileSync 和 Debug 标签
--log-console-tags FileSync,+Debug

# 排除包含 UI 标签的日志
--log-console-tags FileSync,-UI
```

### 多Writer支持
```csharp
new LoggerBuilder()
    .WithLevel(LogLevel.Debug)
    .AddWriter(new ConsoleLogger())
    .AddWriter(new FileLogger("app.log"))
    .Build()
    .IntoGlobalStaticLog();
```

### 内存缓存
```csharp
new LoggerBuilder()
    .WithMemoryCache()  // 在日志系统初始化前也可以使用日志
    .WithLevel(LogLevel.Debug)
    .AddWriter(new ConsoleLogger())
    .Build()
    .IntoGlobalStaticLog();
```

## 5. 在 DotNetCampus.Terminal 中的应用

### 项目配置
项目已在 `Startup.cs` 中完成了日志系统的初始化：

```csharp
.AddSingleton<ILogger>(_ => new LoggerBuilder()
    .WithLevel(LogLevel.Information)
    .AddWriter(new EmptyLogger())
    .Build()
    .IntoGlobalStaticLog())
```

### 使用模式
在项目中，我们采用以下日志记录模式：

```csharp
// 文件同步服务中的日志记录
Log.Info("[FileSync] 开始同步目录 {syncGroup.Name}");
Log.Debug("[FileSync] 正在上传文件: {localFile} -> {remoteFile}");
Log.Error("[FileSync] 同步失败: {error.Message}");

// UI 层的日志记录  
Log.Info("[UI] 用户启动同步操作");
Log.Warn("[UI] 没有启用的同步组，跳过同步");
Log.Error("[UI] 文件同步服务未初始化");
```

### 标签约定
- `[FileSync]`: 文件同步相关操作
- `[UI]`: 用户界面相关操作
- `[SSH]`: SSH 连接相关操作
- `[Config]`: 配置管理相关操作

## 6. 性能优化

### 源生成器优势
- 编译时生成代码，运行时零反射
- 条件编译支持，调试日志在 Release 版本中完全移除
- 内联优化，减少方法调用开销

### 最佳实践
```csharp
// 推荐：使用插值字符串，简洁易读
Log.Info($"[FileSync] 处理文件 {fileName}，大小 {fileSize} 字节");

// 推荐：对于复杂格式化，使用条件判断避免不必要的字符串构造
if (Log.Current.IsEnabled(LogLevel.Debug))
{
    Log.Debug($"[FileSync] 详细进度信息: {GetDetailedProgress()}");
}

// 避免：在循环中频繁记录详细日志
// 建议使用批量日志或者定期日志
```

## 7. 故障排除

### 常见问题

**问题**: 日志没有输出
- 检查日志级别设置是否正确
- 确认 Writer 是否正确添加
- 验证是否调用了 `IntoGlobalStaticLog()`

**问题**: 条件编译日志不生效
- 检查项目的编译条件（DEBUG、TRACE）
- 确认使用的是正确的 Logger（`Log.DebugLogger` 等）

**问题**: 性能问题
- 检查是否在循环中记录过多日志
- 考虑提高日志级别，减少不必要的日志输出
- 使用异步 Writer 处理大量日志

## 8. 与其他日志库的对比

### 优势
- **零依赖**: 使用源生成器，无运行时依赖
- **高性能**: 编译时优化，运行时开销极小
- **简单易用**: 静态方法调用，无需依赖注入
- **条件编译**: 调试日志在发布版本中完全移除

### 适用场景
- 库项目，不希望引入日志依赖
- 性能敏感的应用程序
- 需要简单易用的日志系统
- 多模块项目的日志统一管理

## 总结

DotNetCampus.Logger 通过源生成器技术，提供了一个高性能、零依赖的日志解决方案。在 DotNetCampus.Terminal 项目中，我们充分利用其静态方法调用的便利性和标签过滤功能，实现了结构化的日志记录，便于问题诊断和系统监控。
