# AI 任务分工

## 项目概述

DotNetCampus Terminal 是一个基于 .NET 9.0 和 Terminal.Gui 的远程设备连接管理工具。项目采用多AI协同开发模式，每个AI负责特定的功能模块。

## 技术栈
- .NET 9.0 + C#
- Terminal.Gui (控制台UI)
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
**职责**：负责Terminal.Gui控制台界面设计
**任务**：
- [ ] 设计主界面布局和导航
- [ ] 实现全局菜单栏
- [ ] 实现设备管理界面（个人/团队设备列表）
- [ ] 实现文件夹同步状态显示界面
- [ ] 实现全局命令输入框
- [ ] 优化用户交互体验

**输出文件**：
- `src/DotNetCampus.Terminal/Views/`
- `src/DotNetCampus.Terminal/ViewModels/`

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
- [ ] 深入学习 Terminal.Gui 库的使用技巧和最佳实践
- [ ] 研究 SSH.NET 库的高级功能和性能优化
- [ ] 掌握 Tomlet (TOML解析) 库的配置管理模式
- [ ] 探索 .NET 9.0 的新特性在项目中的应用
- [ ] 整理常见问题和解决方案
- [ ] 维护技术知识库和最佳实践文档

**输出文件**：
- `.github/knowledge/` 目录下的知识文档
- 各库的使用指南和最佳实践
- 常见问题解答文档

### 10. 文档维护员 (Documentation AI)
**职责**：负责项目文档和用户手册
**任务**：
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

