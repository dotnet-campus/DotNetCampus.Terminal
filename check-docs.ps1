# 检查 copilot-instructions.md 中提及的所有文档是否存在
param(
    [string]$InstructionsFile = ".github\copilot-instructions.md",
    [string]$KnowledgeBase = ".github\knowledge"
)

Write-Host "=== 检查 copilot-instructions.md 中引用的文档 ===" -ForegroundColor Green
Write-Host ""

# 检查指令文件是否存在
if (!(Test-Path $InstructionsFile)) {
    Write-Host "❌ 指令文件不存在: $InstructionsFile" -ForegroundColor Red
    exit 1
}

# 读取指令文件内容
$content = Get-Content $InstructionsFile -Raw

# 定义需要检查的文档列表（根据完整的知识库结构更新）
$documents = @(
    # AI任务分工文档
    ".github\AI任务分工.md",
    
    # 角色经验总结文档
    "$KnowledgeBase\AI协作经验\角色经验总结\UI界面设计师-核心经验总结.md",
    "$KnowledgeBase\AI协作经验\角色经验总结\文件同步工程师-核心经验总结.md",
    "$KnowledgeBase\AI协作经验\角色经验总结\配置管理专家-核心经验总结.md",
    "$KnowledgeBase\AI协作经验\角色经验总结\SSH连接专家-核心经验总结.md",
    "$KnowledgeBase\AI协作经验\角色经验总结\文档维护员-核心经验总结.md",
    
    # 依赖库文档
    "$KnowledgeBase\依赖库文档\Consolonia\01-快速参考指南.md",
    "$KnowledgeBase\依赖库文档\Consolonia\02-架构核心要点.md",
    "$KnowledgeBase\依赖库文档\Consolonia\03-UI框架使用.md",
    "$KnowledgeBase\依赖库文档\Consolonia\04-UI设计模式最佳实践.md",
    "$KnowledgeBase\依赖库文档\DotNetCampus.Logger\01-日志框架使用指南.md",
    "$KnowledgeBase\依赖库文档\SSH.NET\01-基础使用指南.md",
    "$KnowledgeBase\依赖库文档\SSH.NET\02-文件同步实现.md",
    "$KnowledgeBase\依赖库文档\Tomlet\01-TOML解析使用指南.md",
    "$KnowledgeBase\依赖库文档\DotNet9\01-新特性在项目中的应用.md",
    
    # 技术设计文档
    "$KnowledgeBase\技术设计文档\界面设计\01-Terminal界面开发指南.md",
    "$KnowledgeBase\技术设计文档\界面设计\02-SSH设备信息视图设计.md",
    "$KnowledgeBase\技术设计文档\界面设计\03-进度显示和数据绑定.md",
    "$KnowledgeBase\技术设计文档\界面设计\04-ViewModel重构最佳实践.md",
    "$KnowledgeBase\技术设计文档\界面设计\05-TUI与Shell集成解决方案.md",
    "$KnowledgeBase\技术设计文档\界面设计\06-交互式命令模式设计.md",
    "$KnowledgeBase\技术设计文档\配置管理\01-TOML配置文件架构设计.md",
    "$KnowledgeBase\技术设计文档\配置管理\02-配置保存功能实现.md",
    "$KnowledgeBase\技术设计文档\配置管理\03-配置数据源迁移方案.md",
    "$KnowledgeBase\技术设计文档\配置管理\04-设备唯一标识符设计.md",
    "$KnowledgeBase\技术设计文档\文件同步\01-远程到本地同步架构.md",
    "$KnowledgeBase\技术设计文档\文件同步\02-增量同步性能优化.md",
    "$KnowledgeBase\技术设计文档\文件同步\03-同步错误处理机制.md",
    "$KnowledgeBase\技术设计文档\SSH连接管理\01-SSH密钥认证配置方案.md",
    "$KnowledgeBase\技术设计文档\SSH连接管理\02-多设备连接安全分析.md",
    
    # 问题排查文档
    "$KnowledgeBase\问题排查\开发问题快速解答手册.md",
    
    # AI协作经验
    "$KnowledgeBase\AI协作经验\实现经验总结\设备唯一ID实现技术总结.md",
    "$KnowledgeBase\AI协作经验\实现经验总结\TOML配置功能实现踩坑记录.md",
    "$KnowledgeBase\AI协作经验\AI多角色协作开发经验.md"
)

# 从文件内容中提取.md文件引用
$mdFilePattern = '`([^`]+\.md)`'
$matches = [regex]::Matches($content, $mdFilePattern)

Write-Host "📋 从文档中发现的 .md 文件引用:" -ForegroundColor Yellow
foreach ($match in $matches) {
    $fileName = $match.Groups[1].Value
    Write-Host "   • $fileName" -ForegroundColor Cyan
}
Write-Host ""

# 检查预定义的文档列表
Write-Host "🔍 检查预定义文档列表:" -ForegroundColor Yellow
$missingCount = 0
$existingCount = 0

foreach ($doc in $documents) {
    if (Test-Path $doc) {
        Write-Host "✅ $doc" -ForegroundColor Green
        $existingCount++
    } else {
        Write-Host "❌ $doc" -ForegroundColor Red
        $missingCount++
    }
}

Write-Host ""

# 检查知识库目录结构
Write-Host "📁 检查知识库目录结构:" -ForegroundColor Yellow
$knowledgeDirs = @(
    "$KnowledgeBase",
    "$KnowledgeBase\AI协作经验",
    "$KnowledgeBase\AI协作经验\角色经验总结",
    "$KnowledgeBase\AI协作经验\实现经验总结",
    "$KnowledgeBase\依赖库文档",
    "$KnowledgeBase\依赖库文档\Consolonia",
    "$KnowledgeBase\依赖库文档\DotNetCampus.Logger",
    "$KnowledgeBase\依赖库文档\SSH.NET",
    "$KnowledgeBase\依赖库文档\Tomlet",
    "$KnowledgeBase\依赖库文档\DotNet9",
    "$KnowledgeBase\技术设计文档",
    "$KnowledgeBase\技术设计文档\界面设计",
    "$KnowledgeBase\技术设计文档\配置管理",
    "$KnowledgeBase\技术设计文档\文件同步",
    "$KnowledgeBase\技术设计文档\SSH连接管理",
    "$KnowledgeBase\问题排查"
)

foreach ($dir in $knowledgeDirs) {
    if (Test-Path $dir -PathType Container) {
        $fileCount = (Get-ChildItem $dir -File -Filter "*.md" | Measure-Object).Count
        Write-Host "✅ $dir ($fileCount 个 .md 文件)" -ForegroundColor Green
    } else {
        Write-Host "❌ $dir (目录不存在)" -ForegroundColor Red
        $missingCount++
    }
}

Write-Host ""

# 列出实际存在的所有 .md 文件
Write-Host "📄 实际存在的知识库文档:" -ForegroundColor Yellow
if (Test-Path $KnowledgeBase) {
    $allMdFiles = Get-ChildItem $KnowledgeBase -Recurse -Filter "*.md" | Sort-Object FullName
    foreach ($file in $allMdFiles) {
        $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")
        Write-Host "   • $relativePath" -ForegroundColor Cyan
    }
} else {
    Write-Host "   知识库目录不存在: $KnowledgeBase" -ForegroundColor Red
}

Write-Host ""

# 总结报告
Write-Host "=== 检查结果总结 ===" -ForegroundColor Green
Write-Host "✅ 存在的文档: $existingCount" -ForegroundColor Green
Write-Host "❌ 缺失的文档: $missingCount" -ForegroundColor Red

if ($missingCount -eq 0) {
    Write-Host "🎉 所有引用的文档都存在！" -ForegroundColor Green
    exit 0
} else {
    Write-Host "⚠️  需要创建或修正缺失的文档" -ForegroundColor Yellow
    exit 1
}
