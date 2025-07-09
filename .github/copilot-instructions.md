# GitHub Copilot 指导文档

DotNetCampus Terminal 项目的 AI 协作开发指南。

## 项目概述

基于 .NET 9.0 和 Consolonia 的远程设备连接管理工具，采用多AI协同开发模式。

**技术栈**：.NET 9.0, Consolonia, SSH.NET, Tomlet, DotNetCampus.Logger

## 核心编码规范

### 命名约定
- 类名/方法名/属性名：PascalCase
- 私有字段：camelCase + 下划线前缀 `_fieldName`
- 接口：以I开头

### 代码风格
- 启用 nullable 引用类型
- 异步方法以 Async 结尾
- 使用依赖注入
- 添加 XML 文档注释

### 兼容性原则
**重要：本项目不需要考虑兼容性问题**
- 不使用 `[Obsolete]` 标记
- 不保留旧方法或接口
- 不添加"兼容旧版本"等注释说明
- 直接重构和替换，无需向后兼容
- 项目处于开发阶段，API可以自由变更

### 代码重构原则
- **400行规则**：代码超过400行时，需要酌情考虑重构
- **600行硬限制**：代码超过600行时，必须考虑重构
- **例外情况**：超过600行但非常单一易懂的代码（如大型枚举）可以保留
- 重构方式：拆分类、提取方法、分离职责

## Consolonia 关键要点

### 核心概念
- **像素 = 字符**：每个像素对应一个控制台字符
- **文件扩展名**：使用 `.axaml` 而不是 `.xaml`
- **命名空间**：`xmlns:console="https://github.com/jinek/consolonia"`
- **主题**：推荐使用 `TurboVisionDarkTheme`

### 重要差异
- 使用 `AvaloniaList<T>` 替代 `ObservableCollection<T>`
- 使用 `console:LineBrush` 配置边框样式
- 按钮禁用阴影：`console:ButtonExtensions.Shadow="False"`
- 异步UI更新：`Dispatcher.UIThread.InvokeAsync`

### 性能优化
- 使用 `VirtualizingStackPanel` 处理大数据集
- 选择合适的绑定模式（OneTime/OneWay/TwoWay）
- 使用 `x:DataType` 实现强类型绑定

## 日志规范

使用 **DotNetCampus.Logger**：
- 使用静态 `Log` 类，无需依赖注入
- 日志格式：`Log.Info("[标签] 消息内容")`
- 标签约定：`[FileSync]` `[SSH]` `[UI]` `[Config]` `[Network]` `[System]`

## 协作要点

### 开发流程
1. 🔥 **首要步骤**：查看 `.github/AI任务分工.md` 确定自己的角色
2. 📖 **必读文档**：阅读对应角色的经验总结文档（如 `UI-Designer-Experience-Summary.md`）
3. 📚 **技术查阅**：查看 `.github/knowledge/` 相关技术文档
4. 接口设计优先，确保模块依赖清晰
5. 及时测试，避免积累错误
6. 知识更新到知识库和经验总结，便于复用

### 角色经验总结文档
- **UI界面设计师**: `UI-Designer-Experience-Summary.md`
- **文件同步工程师**: `File-Sync-Engineer-Experience-Summary.md`
- **配置管理专家**: `Configuration-Expert-Experience-Summary.md`
- **其他角色**: 后续补充对应的经验总结文档

### 求助时机
以下情况建议寻求人类帮助：
- 多个命名空间冲突
- API版本兼容性问题
- 复杂的泛型推断失败
- 平台特定显示问题
- **编译文件占用问题**：当 dotnet build 提示文件被占用时
- **反复犯错**：如果发现自己在重复犯同样的错误

## 技术文档索引

详细的技术资料已整理到 `.github/knowledge/` 目录：

### 角色经验总结（首要阅读）
- `UI-Designer-Experience-Summary.md` - UI设计师核心经验
- `File-Sync-Engineer-Experience-Summary.md` - 文件同步工程师核心经验  
- `Configuration-Expert-Experience-Summary.md` - 配置管理专家核心经验

### 技术参考文档
- `Consolonia-Quick-Reference.md` - Consolonia快速参考
- `DotNetCampus-Logger-Guide.md` - 日志使用指南
- `SSH.NET-File-Sync-Guide.md` - 文件同步指南
- `Terminal-TOML-Configuration-Design.md` - 配置设计
- `常见问题解答.md` - 问题解决方案

**重要提醒**：
1. 🔥 开始任务前，必须先阅读对应角色的经验总结文档
2. 📚 遇到技术问题先查阅知识库，避免重复踩坑
3. 💡 将新的踩坑经验及时更新到经验总结文档中
