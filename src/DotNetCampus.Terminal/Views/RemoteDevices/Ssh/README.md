# SSH设备Views目录

本目录专门存放SSH远程设备相关的View文件。

## 设计原则

### 文件命名规范
- 主View：`SshDeviceMainView.axaml`
- 同步View：`SshDeviceSyncView.axaml` 
- 命令View：`SshDeviceCommandsView.axaml`
- 详情View：`SshDeviceDetailsView.axaml`

### UI组件职责划分

#### SshDeviceSyncView.axaml
负责同步相关的UI组件：
- 同步组列表 (DataGrid/ListBox)
- 全局同步进度条
- 同步状态显示
- 启用/禁用控制
- 错误信息显示

**绑定到**: `Sync` (简化后的属性名)

#### SshDeviceCommandsView.axaml  
负责操作命令相关的UI组件：
- 同步全部按钮
- 取消同步按钮
- 打开Shell按钮
- 保存配置按钮
- 测试连接按钮

**绑定到**: `Commands` (简化后的属性名)

## Consolonia特殊考虑

### 布局约束
- 控制台界面空间有限，优先显示核心功能
- 使用`StackPanel`和`Grid`进行紧凑布局
- 避免过度使用`ScrollViewer`

### 按钮样式
```xml
<Button console:ButtonExtensions.Shadow="False"
        Margin="1" Padding="1,0"
        Content="同步全部" />
```

### 进度条样式
```xml
<ProgressBar Value="{Binding SyncViewModel.GlobalSyncProgress}"
             Minimum="0" Maximum="100"
             Height="1" />
```

## 重构指导

### 何时重构
- 单个View文件超过400行
- UI组件职责混乱
- 数据绑定路径过深

### 重构步骤
1. 分析当前View的功能模块
2. 按ViewModel职责划分UI区域
3. 创建对应的子View文件
4. 更新主View引用子Views
5. 测试数据绑定和命令执行

### 测试清单
- [ ] 数据绑定正常工作
- [ ] 命令按钮可以触发
- [ ] 进度更新实时显示
- [ ] 错误信息正确展示
- [ ] 界面布局美观紧凑
