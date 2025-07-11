# TOML到JSON配置系统迁移记录

## 迁移概述

DotNetCampus Terminal 项目已于 2025年7月 完成从 TOML 配置系统到 JSON 配置系统的完整迁移。

## 技术决策演进

### 配置系统发展历程
1. **Debug阶段**：硬编码配置，用于开发调试
2. **TOML阶段**：使用 `Samboy063.Tomlet` 库，人类可读的配置格式
3. **JSON阶段**：使用 `System.Text.Json` + 源生成器，AOT兼容

## 迁移原因

### TOML 系统的问题
- 🔥 **AOT兼容性问题**：`Tomlet` 库依赖反射，无法在 AOT 环境下工作
- **性能考虑**：JSON 解析性能更优
- **生态系统**：.NET 内置支持，无需额外依赖

### JSON 系统的优势
- ✅ **AOT 兼容**：完全支持源生成器
- ✅ **高性能**：System.Text.Json 优化的序列化性能
- ✅ **零依赖**：.NET 内置库，无需外部包
- ✅ **类型安全**：编译时验证和强类型支持

## 迁移范围

### 已清理的 TOML 组件
- ✅ **包引用**：移除 `Samboy063.Tomlet` 
- ✅ **配置文件**：删除 `devices.toml`
- ✅ **源代码**：删除 `TomlSource` 目录
- ✅ **模型重命名**：`TomlDeviceConfiguration.cs` → `SyncModels.cs`

### 已实现的 JSON 组件
- ✅ **配置源**：`JsonRemoteDeviceConfigurationSource`
- ✅ **数据模型**：`DeviceConfigurationModel` + 源生成器
- ✅ **配置文件**：`devices.json` (AOT兼容格式)

## 技术架构对比

### TOML 架构 (已废弃)
```
TomlRemoteDeviceConfigurationSource
├── Samboy063.Tomlet (第三方库)
├── TomlDeviceConfiguration (数据模型)
└── devices.toml (配置文件)
```

### JSON 架构 (当前)
```
JsonRemoteDeviceConfigurationSource
├── System.Text.Json (内置库)
├── DeviceConfigurationModel (数据模型)
├── JsonSourceGenerationContext (源生成器)
└── devices.json (配置文件)
```

## 配置格式对比

### TOML 格式示例 (历史格式)
```toml
[devices.server-01]
name = "开发服务器"
host = "192.168.1.100"
port = 22
username = "root"

[[devices.server-01.sync_groups]]
name = "项目代码"
remote_path = "/var/www"
local_path = "C:\\Projects"
enabled = true
```

### JSON 格式 (当前格式)
```json
{
  "Devices": [
    {
      "LocalId": "server-01",
      "Name": "开发服务器", 
      "Host": "192.168.1.100",
      "Port": 22,
      "Username": "root",
      "DirectorySyncings": [
        {
          "RemotePath": "/var/www",
          "LocalPath": "C:\\Projects",
          "IsEnabled": true
        }
      ]
    }
  ]
}
```

## 迁移经验总结

### 成功经验
- **渐进式迁移**：先实现 JSON 系统，确保功能完整后再清理 TOML
- **数据保持**：确保用户配置在迁移过程中不丢失
- **向前兼容**：JSON 系统设计时考虑了未来扩展性

### 技术难点
- **AOT 约束**：需要使用源生成器替代反射
- **数据转换**：TOML 到 JSON 的字段映射
- **错误处理**：配置解析异常的统一处理

## 文档清理记录

### 2025年7月11日 - 文档维护员清理行动
**清理原则**：
- 保留一份迁移历史记录（本文档）
- 清理其他文档中的 TOML 引用
- 更新技术栈描述为 JSON 系统

**清理任务分工**：
- **任务1**：技术设计文档清理 (4个文件)
- **任务2**：AI协作经验清理 (3个文件) 
- **任务3**：依赖库和问题文档清理 (2个文件)

## 参考资料

- [JSON配置系统架构设计](../技术设计文档/配置管理/01-JSON配置系统架构设计.md)
- [System.Text.Json使用指南](../依赖库文档/System.Text.Json/01-JSON配置系统使用指南.md)
- [配置管理专家核心经验](../角色经验总结/配置管理专家-核心经验总结.md)

---

> 📝 **文档说明**：本文档记录了 TOML 到 JSON 配置系统的完整迁移过程，作为项目技术决策的历史记录保存。TOML 相关代码已全部清理完成，当前项目完全基于 JSON 配置系统。
