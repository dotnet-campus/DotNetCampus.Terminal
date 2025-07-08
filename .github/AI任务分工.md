# AI 任务分工

你好 AI，当你阅读到这句话，说明你已经成为我们伟大产品开发中的重要一员。相信人类已经给你分配了角色和一个具体的任务，在你后续完成任务的过程中，你需要经常将你的任务进展记录下来，包括：

1. 在完成任务时，打勾
2. 将知识相关的经验总结记录到 `.github/knowledge/` 知识库中对应的文件里（或新建文件）
3. 将其他经验总结记录到 `.github/copilot-instructions.md` 中并保持文件的简洁

另外，再重申一下，你不是一个 AI 在战斗。为了避免 Token 过长导致你的工作效率低下，如果单次任务较多，请拆小后记录到本文件中，这些小任务会被其他 AI 看到并协助你完成。请确保每个小任务都清晰明确，便于其他 AI 理解和接手。

## 项目概述

DotNetCampus Terminal 是一个基于 .NET 9.0 和 Consolonia 的远程设备连接管理工具。项目采用多AI协同开发模式，每个AI负责特定的功能模块。

## 技术栈
- .NET 9.0 + C#
- Consolonia (控制台UI)
- SSH.NET (SSH连接)
- Tomlet (配置管理)

## AI 角色分工

### 1. 架构师 (Architect AI)
**职责**：负责整体架构设计和核心框架
**任务**：
- [ ] 设计项目整体架构和模块划分
- [ ] 完善依赖注入框架 (`Framework/DependencyInjection/`)
- [ ] 设计数据模型和接口定义
- [ ] 制定编码规范和最佳实践
- [ ] 审查其他AI的架构设计决策

**输出文件**：
- `src/DotNetCampus.Terminal/Framework/` 下的架构文件
- `src/DotNetCampus.Terminal/Models/` 数据模型
- `src/DotNetCampus.Terminal/Interfaces/` 接口定义

### 2. 配置管理专家 (Configuration AI)
**职责**：负责配置系统的设计和实现
**任务**：
- [ ] 完善 `ConfigurationManager` 类
- [ ] 实现个人设备配置的存储和管理
- [ ] 实现基于Git仓库的团队设备配置同步
- [ ] 设计配置文件格式（TOML）
- [ ] 实现配置的加密存储（密码等敏感信息）

**输出文件**：
- `src/DotNetCampus.Terminal/Configurations/`
- `src/DotNetCampus.Terminal/Models/Configuration/`

### 3. SSH连接专家 (SSH Connection AI)
**职责**：负责SSH连接功能的实现
**任务**：
- [ ] 封装SSH.NET，实现统一的连接接口
- [ ] 支持Linux SSH连接
- [ ] 支持Mac SSH连接  
- [ ] 实现连接状态监控
- [ ] 实现连接重试和错误处理
- [ ] 设计SSH配置模型

**输出文件**：
- `src/DotNetCampus.Terminal/SshManagement/`
- `src/DotNetCampus.Terminal/Models/Ssh/`

**备注**：可以组建小团队处理复杂的SSH协议相关问题

### 4. 文件同步工程师 (File Sync AI)
**职责**：负责自动文件夹同步功能
**任务**：
- [ ] 实现基于SFTP的文件同步
- [ ] 支持本地到远程的单向同步
- [ ] 支持远程到本地的单向同步
- [ ] 实现同步状态监控和显示
- [ ] 处理文件冲突和错误恢复
- [ ] 实现增量同步优化

**输出文件**：
- `src/DotNetCampus.Terminal/FileSync/`
- `src/DotNetCampus.Terminal/Models/FileSync/`

**备注**：需要与SSH连接专家密切协作

### 5. UI界面设计师 (UI Designer AI)
**职责**：负责Consolonia控制台界面设计
**任务**：
- [x] 设计主界面布局和导航
- [x] 实现SSH设备信息编辑界面
- [x] 实现目录同步配置界面
- [x] 设计状态指示系统
- [x] 实现数据绑定和MVVM模式
- [ ] 实现路径省略功能
- [ ] 添加交互命令和事件处理
- [ ] 实现输入验证和错误提示
- [ ] 优化界面响应性和用户体验

**输出文件**：
- `src/DotNetCampus.Terminal/Views/SshRemoteDeviceInfoView.axaml` ✅
- `src/DotNetCampus.Terminal/ViewModels/SshRemoteDeviceInfoViewModel.cs` ✅
- `src/DotNetCampus.Terminal/ViewModels/SyncGroupViewModel.cs` ✅
- `src/DotNetCampus.Terminal/Views/Converters/` (待实现)

**最新进展** (2025-01-08)：
- ✅ 完成SshRemoteDeviceInfoView界面的完整布局
- ✅ 实现设备基本信息编辑区（连接名称、主机地址、端口、用户名、密码、连接状态）
- ✅ 实现目录同步配置区（同步组列表、状态指示、操作按钮）
- ✅ 完成数据模型和状态系统（SyncGroupViewModel、SyncGroupStatus）
- ✅ 配置状态转换器和数据绑定
- ✅ 程序可正常编译运行，界面符合设计文档要求
- ⏳ 等待用户验收当前进展
~~~~
**技术要点**：
- 使用UniformGrid实现响应式布局
- 使用console:LineBrush绘制分隔线和边框
- 配置连接状态转换器（符号、颜色、文本）
- 实现ListBox和DataTemplate的同步组列表展示
- 使用AvaloniaList<T>替代ObservableCollection<T>
- 配置x:DataType实现强类型绑定

**备注**：界面基础框架已完成，下一步需要实现交互功能和路径省略

### 6. 进程管理专家 (Process Manager AI)
**职责**：负责Shell进程管理和启动
**任务**：
- [ ] 实现在现有终端中启动新Shell的功能
- [ ] 处理不同操作系统的Shell启动逻辑
- [ ] 实现命令执行和进程管理
- [ ] 处理进程间通信
- [ ] 实现连接后的自动化操作

**输出文件**：
- `src/DotNetCampus.Terminal/ProcessManagement/`
- `src/DotNetCampus.Terminal/Shell/`

### 7. Windows连接专家 (Windows Connection AI)
**职责**：负责Windows设备连接功能
**任务**：
- [ ] 研究Windows远程连接方案
- [ ] 实现Windows服务程序（如需要）
- [ ] 实现Windows特有的连接协议
- [ ] 处理Windows权限和安全问题
- [ ] 与其他连接模块保持接口一致性

**输出文件**：
- `src/DotNetCampus.Terminal/WindowsConnection/`
- 可能需要独立的Windows服务项目

### 8. 测试工程师 (Test Engineer AI)
**职责**：负责测试框架和测试用例
**任务**：
- [ ] 设计测试架构和测试策略
- [ ] 编写单元测试
- [ ] 编写集成测试
- [ ] 实现自动化测试
- [ ] 性能测试和压力测试
- [ ] 建立CI/CD流程

**输出文件**：
- `tests/` 目录下的所有测试项目
- `.github/workflows/` CI配置

### 9. 知识学习者 (Knowledge Learning AI)
**职责**：负责学习项目依赖库并分享知识
**任务**：
- [x] 深入学习 Consolonia 库的使用技巧和最佳实践
- [x] 学习 Consolonia 官方架构文档和主题系统
- [ ] 研究 SSH.NET 库的高级功能和性能优化
- [ ] 掌握 Tomlet (TOML解析) 库的配置管理模式
- [ ] 探索 .NET 9.0 的新特性在项目中的应用
- [ ] 整理常见问题和解决方案
- [x] 维护技术知识库和最佳实践文档

**输出文件**：
- `.github/knowledge/` 目录下的知识文档
- 各库的使用指南和最佳实践
- 常见问题解答文档

**最新进展**：
- ✅ 已完成 Consolonia UI 框架的深度学习和文档整理
- ✅ 创建了 `Consolonia-UI-Framework.md` - 完整的框架使用指南
- ✅ 创建了 `Consolonia-UI-Design-Patterns.md` - UI设计模式和最佳实践
- ✅ 创建了 `DotNetCampus-Terminal-UI-Development-Guide.md` - 项目特定的UI开发指南
- ✅ 分析了现有UI代码结构和实现模式
- ✅ 为UI界面设计师提供了完整的技术支持文档
- ✅ 学习了 Consolonia 官方架构文档，创建了 `Consolonia-Architecture-Essential.md`
- ✅ 更新了快速参考指南，加入了主题系统和绘制系统的最新知识

### 10. 文档维护员 (Documentation AI)
**职责**：负责项目文档和用户手册
**任务**：
- [x] 清理旧的 Terminal.Gui 知识库文档
- [x] 更新技术栈信息为 Consolonia
- [ ] 维护README.md和开发文档
- [ ] 编写用户使用手册
- [ ] 维护API文档
- [ ] 编写部署和配置指南
- [ ] 维护更新日志

**输出文件**：
- `docs/` 目录下的文档
- README.md 更新
- Wiki 页面

## 协作流程

1. **知识学习者**首先深入学习各个依赖库，建立知识库
2. **架构师**完成核心架构设计
3. **配置管理专家**和**SSH连接专家**并行开发基础功能
4. **文件同步工程师**在SSH功能完成后开始开发
5. **UI界面设计师**可以并行开发界面原型
6. **进程管理专家**和**Windows连接专家**根据需要加入
7. **测试工程师**在各模块完成后进行测试
8. **文档维护员**持续更新项目文档
9. **知识学习者**持续更新技术知识库

## 沟通机制

- 每个AI在开始工作前应查看相关依赖模块的接口定义
- 重要的架构决策需要通过**架构师**审查
- 接口变更需要通知所有相关AI
- 技术问题和解决方案及时更新到**知识库**中
- 定期同步进度和问题

## 交付标准

- 代码符合项目编码规范
- 包含必要的单元测试
- 代码有充分的注释和文档
- 通过所有自动化测试
- 接口设计合理，易于扩展
