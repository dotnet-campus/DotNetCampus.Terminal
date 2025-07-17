# SshRemoteDeviceInfoView Avalonia GUI 设计文档

## 设计概述
SSH 远程设备详细信息编辑界面，采用现代化 Avalonia GUI 设计，包含设备连接信息编辑和目录同步配置。

## 整体布局
```
┌─────────────────────────────────────────────────────────────┐
│                    设备基本信息卡片                           │
│  ┌─ 连接信息 ──────────────────────────────────────────┐    │
│  │  [连接名称输入框]              [🔄连接状态指示]       │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌─────────┐    │    │
│  │  │  主机地址    │  │    端口      │  │  用户名  │    │    │
│  │  └──────────────┘  └──────────────┘  └─────────┘    │    │
│  │  ┌──────────────┐  ┌──────────────┐                 │    │
│  │  │    密码      │  │  私钥文件    │                 │    │
│  │  └──────────────┘  └──────────────┘                 │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐ │    │
│  │  │  [🔗连接]   │  │  [💾保存]   │  │  [🔄重置]   │ │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘ │    │
│  └─────────────────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│                    目录同步管理卡片                           │
│  ┌─ 文件同步配置 ──────────────────────────────────────┐    │
│  │  ┌──────────────────────────────────────────────────┐   │
│  │  │ 📂 MyVeryLongProjectName    [✅][⚙️][❌]         │   │
│  │  │    远程: /home/user/projects/myproject/...       │   │
│  │  │    本地: D:\Projects\MyVeryLongProjectName\...   │   │
│  │  │    状态: 🟢 同步正常 | 上次同步: 2分钟前          │   │
│  │  ├──────────────────────────────────────────────────┤   │
│  │  │ 📂 VeryLongDepartment       [⚠️][⚙️][❌]         │   │
│  │  │    远程: /home/user/documents/work/...           │   │
│  │  │    本地: D:\Documents\Work\VeryLongDepartment\   │   │
│  │  │    状态: 🔴 同步出错 | 错误: 权限不足             │   │
│  │  └──────────────────────────────────────────────────┘   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │  │
│  │  │ [➕添加同步] │  │ [✅全部启用] │  │ [❌全部禁用] │    │  │
│  │  └─────────────┘  └─────────────┘  └─────────────┘    │  │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## 核心UI设计要点

### 1. 设备信息卡片区
- **现代化卡片布局**：使用 Border 和 CornerRadius 实现卡片效果
- **分组输入**：连接信息和认证信息逻辑分组
- **实时状态指示**：🔴断开 🟡连接中 🟢已连接 ⚠️错误
- **输入验证**：实时验证和错误提示，支持高亮显示
- **多种认证方式**：密码认证、私钥认证的切换

### 2. 目录同步管理区
- **卡片式列表**：每个同步项目使用独立卡片显示
- **丰富状态显示**：状态图标、进度指示、错误信息
- **操作按钮组**：启用/禁用、设置、删除操作
- **智能路径显示**：自动缩略过长路径
- **同步状态监控**：实时显示同步进度和状态

### 3. 目录同步项目设计
- **名称编辑**：双击进入编辑模式，支持重命名
- **路径显示**：本地和远程路径的清晰对比
- **状态监控**：同步状态、上次同步时间、错误信息
- **操作便捷性**：快速启用/禁用、设置、删除

### 4. 响应式设计
- **自适应布局**：根据窗口大小调整布局
- **路径智能截断**：根据可用宽度调整路径显示
- **弹性间距**：合理的元素间距和内边距

### 5. 交互体验
- **快捷键支持**：Ctrl+S 保存、F2 重命名、Delete 删除
- **拖拽支持**：支持文件夹拖拽添加同步
- **上下文菜单**：右键菜单提供完整操作选项
- **工具提示**：悬停显示完整信息

## 技术实现要点

### 1. 核心控件和布局
- **ScrollViewer**：外层容器，支持滚动浏览
- **StackPanel**：主要布局容器，垂直排列卡片
- **Border + CornerRadius**：卡片容器，实现现代化卡片效果
- **Grid**：内部精确布局，支持响应式列宽
- **TextBox + ValidatedTextBox**：输入控件，支持验证和错误显示

### 2. 样式和主题
- **Fluent 设计系统**：遵循现代化设计语言
- **动态资源**：使用 `{DynamicResource}` 支持主题切换
- **卡片样式**：统一的阴影、圆角、边框设计
- **状态颜色**：语义化的颜色系统

### 3. 数据绑定和验证
- **强类型绑定**：使用 `x:DataType` 提高性能
- **双向绑定**：实时编辑和保存
- **验证系统**：IDataErrorInfo 或 INotifyDataErrorInfo
- **转换器**：状态到颜色、图标的转换

### 4. 现代化 XAML 示例
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:DataType="vm:SshRemoteDeviceInfoViewModel">
    
    <ScrollViewer>
        <StackPanel Spacing="16" Margin="16">
            <!-- 连接信息卡片 -->
            <Border Background="{DynamicResource CardBackground}"
                    BorderBrush="{DynamicResource CardBorderBrush}"
                    BorderThickness="1"
                    CornerRadius="8"
                    Padding="16">
                <StackPanel Spacing="12">
                    <TextBlock Text="连接信息" 
                               Classes="h4"
                               Foreground="{DynamicResource TextFillColorPrimary}" />
                    
                    <UniformGrid Columns="3" ColumnSpacing="12">
                        <TextBox Watermark="主机地址" 
                                 Text="{Binding HostAddress, Mode=TwoWay}" />
                        <NumericUpDown Watermark="端口" 
                                       Value="{Binding Port, Mode=TwoWay}"
                                       Minimum="1" Maximum="65535" />
                        <TextBox Watermark="用户名" 
                                 Text="{Binding Username, Mode=TwoWay}" />
                    </UniformGrid>
                    
                    <StackPanel Orientation="Horizontal" Spacing="12">
                        <Button Classes="accent" Command="{Binding ConnectCommand}">
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <PathIcon Data="{StaticResource ConnectIcon}" />
                                <TextBlock Text="连接" />
                            </StackPanel>
                        </Button>
                        <Button Command="{Binding SaveCommand}">
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <PathIcon Data="{StaticResource SaveIcon}" />
                                <TextBlock Text="保存" />
                            </StackPanel>
                        </Button>
                    </StackPanel>
                </StackPanel>
            </Border>
            
            <!-- 目录同步卡片 -->
            <Border Background="{DynamicResource CardBackground}"
                    BorderBrush="{DynamicResource CardBorderBrush}"
                    BorderThickness="1"
                    CornerRadius="8"
                    Padding="16">
                <StackPanel Spacing="12">
                    <TextBlock Text="文件同步配置" 
                               Classes="h4"
                               Foreground="{DynamicResource TextFillColorPrimary}" />
                    
                    <ItemsControl ItemsSource="{Binding SyncItems}">
                        <ItemsControl.ItemTemplate>
                            <DataTemplate x:DataType="vm:DirectorySyncItemViewModel">
                                <Border Background="{DynamicResource SubtleBackgroundSecondary}"
                                        BorderBrush="{DynamicResource DividerStrokeColorDefault}"
                                        BorderThickness="1"
                                        CornerRadius="4"
                                        Padding="12"
                                        Margin="0,4">
                                    <!-- 同步项内容 -->
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</UserControl>
```

## 实现进展记录

### 第一阶段：现代化设备信息卡片 ✅
**时间**: 2025-07-17

#### 完成内容
1. **卡片式布局**: 使用 Border + CornerRadius 实现现代化卡片效果
2. **连接状态指示**: 美化显示，使用语义化图标（🔴🟡🟢⚠️）和颜色
3. **分组输入字段**: 连接信息、认证信息分组，使用 Grid 和 UniformGrid 布局
4. **现代化按钮**: 带图标的操作按钮，支持 accent 样式

#### 技术实现
- 使用 `StackPanel + Spacing` 实现垂直布局
- 使用 `DynamicResource` 支持主题切换
- 配置连接状态转换器（图标、颜色、文本）
- 实现强类型数据绑定和验证系统

### 第二阶段：现代化同步管理卡片 ✅
**时间**: 2025-07-17

#### 完成内容
1. **卡片分隔**: 使用现代化卡片边框替代传统分隔线
2. **同步项列表**: 使用 ItemsControl 和 DataTemplate 实现
3. **丰富状态显示**: 包含图标、状态文本、进度指示、错误信息
4. **操作按钮组**: 图标化的启用、设置、删除按钮

#### 技术实现
- 创建 `DirectorySyncItemViewModel` 强类型视图模型
- 实现 `SyncStatus` 枚举和现代化状态指示
- 使用 `ItemsControl` 和自定义 `DataTemplate` 
- 配置响应式布局和动态主题资源

### 第三阶段：数据模型和现代化状态系统 ✅
**时间**: 2025-07-17

#### 完成内容
1. **DirectorySyncItemViewModel**: 现代化同步项视图模型，支持丰富状态
2. **SyncStatus**: 同步状态枚举（Normal/Syncing/Error/Disabled）
3. **图标化状态指示**: 使用 PathIcon 和语义化图标
4. **设计时数据**: 现代化示例数据用于界面预览

#### 技术实现
- 使用 `BindableRecord` 作为 ViewModel 基类
- 实现现代化属性变更通知机制
- 使用 `ObservableCollection<T>` 支持集合变更通知
- 配置主题资源和图标系统

### 当前界面布局结构
```
┌─────────────────────────────────────────┐
│ ╭─ 连接信息 ──────────────────────────╮ │
│ │   [设备名称输入框]    🟢 已连接      │ │
│ │   ┌─────────────┐ ┌─────┐ ┌─────────┐│ │
│ │   │  主机地址   │ │端口 │ │ 用户名  ││ │
│ │   └─────────────┘ └─────┘ └─────────┘│ │
│ │   ┌─────────────┐ ┌─────────────────┐│ │
│ │   │    密码     │ │   私钥文件      ││ │
│ │   └─────────────┘ └─────────────────┘│ │
│ │   [🔗连接] [💾保存] [🔄重置]          │ │
│ ╰─────────────────────────────────────╯ │
├─────────────────────────────────────────┤
│ ╭─ 文件同步配置 ──────────────────────╮ │
│ │ ╭─────────────────────────────────╮   │ │
│ │ │ 📂 MyProject  [✅][⚙️][❌]      │   │ │
│ │ │ 远程: /home/user/projects/...   │   │ │
│ │ │ 本地: D:\Projects\MyProject\... │   │ │
│ │ │ 状态: 🟢 同步正常 | 2分钟前       │   │ │
│ │ ╰─────────────────────────────────╯   │ │
│ │ ╭─────────────────────────────────╮   │ │
│ │ │ 📂 Department   [⚠️][⚙️][❌]     │   │ │
│ │ │ 远程: /home/user/docs/work/...  │   │ │
│ │ │ 本地: D:\Documents\Work\...     │   │ │
│ │ │ 状态: 🔴 权限不足 | 5分钟前       │   │ │
│ │ ╰─────────────────────────────────╯   │ │
│ │ [➕添加同步] [✅全部启用] [❌全部禁用] │ │
│ ╰─────────────────────────────────────╯ │
└─────────────────────────────────────────┘
```

### 下一步计划
1. **智能路径显示** - 实现路径的响应式截断和工具提示
2. **拖拽交互** - 支持文件夹拖拽添加同步配置
3. **同步设置对话框** - 实现详细的同步参数配置界面
4. **输入验证增强** - 添加实时验证和错误高亮
5. **动画和过渡** - 添加平滑的状态切换动画
6. **键盘导航** - 完善的快捷键和键盘操作支持

### 当前状态
- ✅ 程序可以正常编译运行
- ✅ 现代化界面布局符合 Avalonia GUI 设计规范
- ✅ 强类型数据绑定工作正常
- ✅ 主题资源和图标系统完整
- ✅ 响应式布局适配不同窗口大小
- ⏳ 等待用户验收 Avalonia GUI 迁移成果
