# 远程设备Views目录

本目录用于存放远程设备相关的View文件。

## 目录结构设计

```
Views/RemoteDevices/
├── README.md                          # 本文件
├── Ssh/                              # SSH设备专用Views
│   └── README.md                     # SSH Views说明
└── [未来可能的其他设备类型]/
    ├── Windows/                      # Windows远程连接Views
    └── PeerToPeer/                   # 对等连接Views
```

## 重构计划

### 当前状态
- `SshRemoteDeviceInfoView.axaml` 文件过长，需要拆分
- 主View位于 `Views/SshRemoteDeviceInfoView.axaml`

### 重构目标
当 `SshRemoteDeviceInfoView.axaml` 文件超过400行时，建议按以下方式拆分：

1. **主View** (`Views/SshRemoteDeviceInfoView.axaml`)
   - 保留基础设备信息展示
   - 包含子Views的引用

2. **同步相关View** (`Views/RemoteDevices/Ssh/SshDeviceSyncView.axaml`)
   - 同步组列表
   - 同步进度显示
   - 同步控制按钮

3. **命令相关View** (`Views/RemoteDevices/Ssh/SshDeviceCommandsView.axaml`)
   - 连接测试
   - Shell打开
   - 配置保存
   - 其他操作按钮

## ViewModel对应关系

```
SshRemoteDeviceInfoViewModel (主ViewModel)
├── SyncViewModel (同步相关)        → SshDeviceSyncView
└── CommandsViewModel (命令相关)    → SshDeviceCommandsView
```

## 重构经验总结

### 成功经验
1. **文件夹结构分层**：按设备类型和功能分层组织
2. **ViewModel职责分离**：拆分为同步、命令等独立职责
3. **保持属性变更通知**：避免使用委托属性，直接绑定到子ViewModels

### 注意事项
1. **编译错误处理**：重构时注意使用`record`而非`class`
2. **依赖注入时机**：确保子ViewModels在使用前已正确初始化
3. **UI绑定更新**：重构后需要更新View中的绑定路径

## 示例绑定更新

重构前：
```xml
<TextBlock Text="{Binding GlobalSyncProgress}" />
```

重构后：
```xml
<TextBlock Text="{Binding SyncViewModel.GlobalSyncProgress}" />
```
