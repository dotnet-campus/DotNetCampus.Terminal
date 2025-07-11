# ViewModel重构最佳实践

本文档总结了SshRemoteDeviceInfoViewModel重构的经验和最佳实践。

## 重构背景

`SshRemoteDeviceInfoViewModel`原始文件有585行，超过了400行的重构建议，需要进行拆分优化。

## 重构策略

### 文件夹结构设计

```
ViewModels/
├── SshRemoteDeviceInfoViewModel.cs     # 主ViewModel（精简版）
└── RemoteDevices/                      # 远程设备相关
    └── Ssh/                           # SSH设备专用
        ├── SshDeviceSyncViewModel.cs   # 同步功能
        └── SshDeviceCommandsViewModel.cs # 命令处理

Views/
├── SshRemoteDeviceInfoView.axaml       # 主View
└── RemoteDevices/                      # 远程设备Views（预留）
    ├── README.md                      # 目录说明
    └── Ssh/                          # SSH设备Views（预留）
        └── README.md                 # SSH说明
```

### 职责分离原则

#### 主ViewModel职责
- 基础设备属性（连接名、主机、端口、用户名、密码）
- 子ViewModels的组合和初始化
- 连接测试功能

#### 同步ViewModel职责
- 目录同步管理
- 同步进度追踪
- 同步状态管理
- 错误信息处理

#### 命令ViewModel职责
- 所有按钮命令的处理
- 与外部服务的交互
- 配置保存逻辑

## 重构要点

### 1. 避免委托属性
**错误做法**：
```csharp
public double GlobalSyncProgress => SyncViewModel.GlobalSyncProgress;
```

**正确做法**：
```csharp
public SshDeviceSyncViewModel SyncViewModel { get; }
```

**理由**：委托属性不会触发PropertyChanged事件，导致UI绑定失效。

### 2. ViewModel类型选择
**规则**：在本项目中使用`record`而非`class`

```csharp
public partial record SshDeviceSyncViewModel : BindableRecord
```

### 3. 初始化顺序
**关键**：确保依赖关系正确

```csharp
public SshRemoteDeviceInfoViewModel(SshRemoteDeviceInfo info) : base(info)
{
    // 1. 先初始化基础属性
    _connectionName = info.ConnectionName;
    
    // 2. 再初始化子ViewModels
    var fileSyncService = Container.Current.EnsureGet<IFileSyncService>();
    SyncViewModel = new SshDeviceSyncViewModel(fileSyncService);
    CommandsViewModel = new SshDeviceCommandsViewModel(SyncViewModel, GetCurrentDeviceInfo);
    
    // 3. 最后加载数据
    SyncViewModel.InitializeDirectorySyncing(info.SyncDirectories);
}
```

### 4. UI绑定更新
**重构前**：
```xml
<TextBlock Text="{Binding GlobalSyncProgress}" />
<Button Command="{Binding SyncAllCommand}" />
```

**重构后**：
```xml
<TextBlock Text="{Binding SyncViewModel.GlobalSyncProgress}" />
<Button Command="{Binding CommandsViewModel.SyncAllCommand}" />
```

## UI绑定优化

### 子ViewModel属性命名优化

**问题**：重构后的绑定路径过于冗长
```xml
<!-- 不优雅的绑定 -->
<TextBlock Text="{Binding SyncViewModel.GlobalSyncProgress}" />
<Button Command="{Binding CommandsViewModel.SyncAllCommand}" />
```

**解决方案**：使用简洁的属性名
```csharp
// 主ViewModel中的简洁命名
public SshDeviceSyncViewModel Sync { get; }
public SshDeviceCommandsViewModel Commands { get; }
```

```xml
<!-- 优雅的绑定 -->
<TextBlock Text="{Binding Sync.GlobalSyncProgress}" />
<Button Command="{Binding Commands.SyncAllCommand}" />
```

**命名规范**：
- 同步相关：`Sync`
- 命令相关：`Commands`  
- 配置相关：`Config`
- 状态相关：`Status`

**优势**：
1. **绑定路径更简洁**：`Sync.Progress` vs `SyncViewModel.Progress`
2. **代码可读性更好**：语义更清晰
3. **重构更容易**：属性名变更影响范围小
4. **团队一致性**：建立统一的命名规范

## 编译错误解决

### 1. BindableRecord vs ViewModelBase
**错误**：
```
未能找到类型或命名空间名"ViewModelBase"
```

**解决**：使用项目中的`BindableRecord`基类

### 2. init-only属性问题
**错误**：
```
只能在对象初始值设定项中分配 init-only 属性
```

**解决**：使用`with`表达式创建新实例
```csharp
var updatedDeviceInfo = currentDeviceInfo with
{
    SyncDirectories = _syncViewModel.GetDirectorySyncingModels()
};
```

### 3. class vs record
**错误**：
```
只有记录可以从记录继承
```

**解决**：继承`BindableRecord`时使用`record`关键字

## 性能优化考虑

### 1. 避免过度拆分
- 不要为了拆分而拆分
- 保持功能的内聚性
- 考虑UI绑定的复杂度

### 2. 合理使用依赖注入
- 避免在构造函数中做重工作
- 延迟初始化非关键组件
- 使用工厂模式处理复杂依赖

### 3. 内存管理
- 正确处理事件订阅
- 及时释放CancellationTokenSource
- 避免循环引用

## 测试验证

### 编译测试
```powershell
dotnet build "DotNetCampus.Terminal.csproj"
```

### 功能测试清单
- [ ] 基础属性绑定正常
- [ ] 同步功能正常工作
- [ ] 命令按钮响应正确
- [ ] 错误信息正确显示
- [ ] 配置保存成功
- [ ] 连接测试有效

## 后续优化方向

1. **View层重构**：当`SshRemoteDeviceInfoView.axaml`超过400行时进行拆分
2. **单元测试**：为拆分后的ViewModels添加单元测试
3. **性能监控**：添加性能指标收集
4. **错误处理**：完善异常处理和用户友好的错误提示

## 总结

本次重构成功将585行的大ViewModel拆分为：
- 主ViewModel：112行
- 同步ViewModel：286行  
- 命令ViewModel：176行

总体代码结构更清晰，职责更明确，便于后续维护和扩展。
