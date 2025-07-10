# DevOps 自动化专家 - 核心经验总结

**角色职责**: 负责 GitHub Actions CI/CD 流水线设计、跨平台构建、自动化发布和代码质量检查

## ✅ 已完成任务总结

### 1. GitHub Actions CI/CD 流水线设计和实现
- ✅ 创建了完整的 CI 流水线 (`.github/workflows/ci.yml`)
- ✅ 支持跨平台构建验证 (Windows/Linux/macOS)
- ✅ 集成了 .NET 9.0 支持和 AOT 发布验证
- ✅ 基于 push 和 pull request 触发

### 2. 跨平台应用程序自动化构建
- ✅ 实现了基于 Tag 的自动化发布流水线 (`.github/workflows/release.yml`)
- ✅ 支持三大平台：Windows (win-x64)、Linux (linux-x64)、macOS (osx-x64)
- ✅ 使用 PublishAot=true 进行 AOT 编译
- ✅ 自动重命名可执行文件为平台特定名称
- ✅ **修复发布产物上传问题**: 解决了 .exe.zip 后缀和 404 链接问题

### 3. 自动化发布和版本管理
- ✅ 集成了 dotnetCampus.TagToVersion 工具
- ✅ 版本号自动从 Git Tag 提取并写入 Version.props
- ✅ 支持预发布版本识别 (alpha/beta/rc)
- ✅ 自动创建 GitHub Releases 并上传构建产物
- ✅ **优化发布页面**: 统一英文描述，提升国际化标准

### 4. 代码质量检查和自动化测试集成
- ✅ 创建了代码质量检查流水线 (`.github/workflows/code-quality.yml`)
- ✅ 集成了 CodeQL 安全扫描
- ✅ 添加了依赖项审查 (Dependency Review)
- ❌ 代码格式检查已移除（团队尚未统一 .editorconfig 标准）

### 5. 依赖项安全扫描和更新
- ✅ 创建了自动化依赖更新流水线 (`.github/workflows/dependency-update.yml`)
- ✅ 每周一自动检查过期的 NuGet 包
- ✅ 自动创建 PR 进行依赖更新

### 6. 基础设施即代码
- ❌ EditorConfig 文件已删除（团队尚未统一代码格式标准）
- ✅ 设置了 CHANGELOG.md 模板
- ✅ 在 README.md 中添加了 CI 状态徽章

## 🔧 核心技术要点

### GitHub Actions 最佳实践
```yaml
# 1. 使用最新版本的 Actions
- uses: actions/checkout@v4
- uses: actions/setup-dotnet@v4

# 2. 跨平台路径处理
- name: Publish (Windows)
  if: matrix.os == 'windows-latest'
  run: dotnet publish .\src\DotNetCampus.Terminal\ -p:PublishAot=true -r ${{ matrix.runtime }}

- name: Publish (Unix)
  if: matrix.os != 'windows-latest'
  run: dotnet publish ./src/DotNetCampus.Terminal/ -p:PublishAot=true -r ${{ matrix.runtime }}

# 3. 条件执行和矩阵策略 (修正版)
strategy:
  matrix:
    include:
      - os: windows-latest
        runtime: win-x64
        artifact-name: DotNetCampus.Terminal-win-x64        # 用于 zip 文件名
        executable-name: DotNetCampus.Terminal-win-x64.exe  # 用于可执行文件名
```

### 版本管理策略
- 使用 `dotnetCampus.TagToVersion` 工具从 Git Tag 自动提取版本号
- 版本号格式：`v{major}.{minor}.{patch}[-{prerelease}]`
- 预发布版本自动识别：`alpha`、`beta`、`rc`

### 构建产物策略
- 使用 AOT 编译 (`-p:PublishAot=true`) 提高性能
- 生成 self-contained 单文件可执行程序
- 平台特定命名：
  - Windows: `DotNetCampus.Terminal-win-x64.exe`
  - Linux: `DotNetCampus.Terminal-linux-x64`
  - macOS: `DotNetCampus.Terminal-osx-x64`

## ⚠️ 踩坑经验

### 1. 跨平台路径问题
**问题**: Windows 使用反斜杠 `\`，Unix 系统使用正斜杠 `/`
**解决方案**: 
- 使用条件执行分别处理不同平台
- 在 matrix 中定义 `path-separator` 变量

### 2. 文件重命名问题
**问题**: 不同平台的文件重命名命令不同
**解决方案**:
```yaml
# Windows (PowerShell)
- name: Rename executable (Windows)
  if: matrix.os == 'windows-latest'
  run: |
    if (Test-Path ".\publish\${{ matrix.runtime }}\DotNetCampus.Terminal.exe") {
      Rename-Item ".\publish\${{ matrix.runtime }}\DotNetCampus.Terminal.exe" "${{ matrix.executable-name }}"
    }
  shell: pwsh

# Unix (Bash)
- name: Rename executable (Unix)
  if: matrix.os != 'windows-latest'
  run: |
    if [ -f "./publish/${{ matrix.runtime }}/DotNetCampus.Terminal" ]; then
      mv "./publish/${{ matrix.runtime }}/DotNetCampus.Terminal" "./publish/${{ matrix.runtime }}/${{ matrix.executable-name }}"
    fi
```

### 3. Artifacts 上传路径问题
**问题**: 需要同时支持 Windows 和 Unix 路径格式
**解决方案**: 在 `upload-artifact` 中同时指定两种路径格式

### 4. 代码格式检查的团队协作问题 ⭐
**问题**: 团队对 .editorconfig 标准尚未统一，强制格式检查可能导致CI失败
**解决方案**: 
- 暂时移除 .editorconfig 文件和相关的格式检查步骤
- 等待团队统一标准后再重新引入
- 保留 CodeQL 和依赖项检查等核心安全功能

**经验教训**: 在多人协作项目中，代码格式标准化需要全团队讨论决定，不能单方面强制实施

### 5. GitHub Release 产物上传问题 ⭐⭐⭐
**问题**: Release 页面只有 Windows 版本，其他平台缺失且文件链接 404
**根本原因**:
1. 只上传了单个可执行文件，没有上传完整的发布包
2. 文件夹结构包含多余的外层目录
3. artifacts 下载路径不正确
4. **命名规范错误**: `DotNetCampus.Terminal-win-x64.exe` 作为 artifact-name 导致生成 `.exe.zip` 后缀

**解决方案**:
```yaml
# 1. 分离 artifact-name 和 executable-name
matrix:
  include:
    - os: windows-latest
      runtime: win-x64
      artifact-name: DotNetCampus.Terminal-win-x64        # 用于 zip 文件名
      executable-name: DotNetCampus.Terminal-win-x64.exe  # 用于可执行文件名

# 2. 创建完整的 zip 包，包含所有发布文件
- name: Create release package (Windows)
  if: matrix.os == 'windows-latest'
  run: |
    # 创建临时文件夹，复制所有发布文件
    New-Item -ItemType Directory -Path ".\temp-release" -Force
    Copy-Item -Path ".\publish\${{ matrix.runtime }}\*" -Destination ".\temp-release\" -Recurse
    # 创建 zip 包，不包含外层文件夹
    Compress-Archive -Path ".\temp-release\*" -DestinationPath ".\${{ matrix.artifact-name }}.zip"

# 3. 上传 zip 文件而不是单个可执行文件
- name: Upload artifacts
  uses: actions/upload-artifact@v4
  with:
    name: ${{ matrix.artifact-name }}
    path: ${{ matrix.artifact-name }}.zip

# 4. 正确的文件路径引用
files: |
  ./artifacts/DotNetCampus.Terminal-win-x64/DotNetCampus.Terminal-win-x64.zip
  ./artifacts/DotNetCampus.Terminal-linux-x64/DotNetCampus.Terminal-linux-x64.zip
  ./artifacts/DotNetCampus.Terminal-osx-x64/DotNetCampus.Terminal-osx-x64.zip
```

**经验教训**: 
- AOT 发布的应用程序除了主程序外，还可能包含其他必要文件
- 应该打包完整的发布目录而不是只上传可执行文件
- GitHub Actions 的 artifacts 下载会创建以 artifact name 命名的文件夹
- **命名规范很重要**: artifact-name 不应包含文件扩展名，否则会产生混乱的后缀

### 6. 跨平台 zip 创建兼容性问题
**问题**: Windows 和 Unix 系统的压缩命令不同
**解决方案**:
```yaml
# Windows: 使用 PowerShell 的 Compress-Archive
Compress-Archive -Path ".\temp-release\*" -DestinationPath ".\${{ matrix.artifact-name }}.zip"

# Unix: 使用 zip 命令，注意工作目录
cd temp-release && zip -r ../${{ matrix.artifact-name }}.zip * && cd ..
```

**注意事项**: 
- zip 命令需要进入目录内部执行，避免包含外层文件夹
- PowerShell 的 Compress-Archive 直接指定源路径和目标路径

### 7. Release 页面国际化标准 ⭐
**问题**: 发布页面中英文混杂，不符合国际开源项目标准
**解决方案**: 
- 统一使用英文描述，提升项目专业性
- 保持表格格式清晰，便于用户快速定位下载链接
- 提供详细的安装指南，覆盖各个平台

**经验教训**: 开源项目的发布页面应该保持国际化标准，使用英文可以让更多开发者理解和使用

## 📋 待实现功能

### 高级特性 (未来考虑)
- [ ] 增量构建优化
- [ ] 性能基准测试自动化
- [ ] 自动化变更日志生成
- [ ] 代码签名 (Windows 平台)
- [ ] 集成测试和 UI 自动化测试 (由其他 AI 负责)

## 🤝 与其他角色的协作

- **测试工程师**: 将来需要集成自动化测试到 CI 流水线
- **文档维护员**: 协作维护 CHANGELOG.md 和发布文档
- **所有开发AI**: 确保代码质量检查通过

## 📞 需要人类协助的事项

以下事项需要项目管理员在 GitHub 上配置：

1. **GitHub Secrets 配置**:
   - 确保 `GITHUB_TOKEN` 具有 Release 创建权限
   - 如需代码签名，需要配置相关证书 secrets

2. **Branch Protection Rules**:
   - 设置 main 分支保护规则
   - 要求 CI 检查通过才能合并

3. **GitHub Actions 权限**:
   - 确保 Actions 有权限创建 Releases
   - 确保 Actions 有权限执行 CodeQL 扫描

4. **项目设置**:
   - 启用 Vulnerability alerts
   - 启用 Dependency graph
   - 配置 Security advisories

## 💡 持续改进建议

1. **监控和告警**: 考虑集成构建失败通知
2. **缓存优化**: 添加 NuGet 包缓存以加速构建
3. **并行化**: 进一步优化构建并行度
4. **环境隔离**: 考虑使用容器化构建环境

---

**最后更新**: 2025年7月10日
**负责AI**: DevOps 自动化专家
