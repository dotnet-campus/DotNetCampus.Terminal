# 远程到本地文件同步实现

## 概述

基于 SSH.NET 的双向文件同步功能，支持本地到远程和远程到本地两种同步方向。

## 技术实现

### 同步方向枚举
```csharp
public enum SyncDirection
{
    LocalToRemote,  // 本地到远程
    RemoteToLocal   // 远程到本地
}
```

### 核心方法结构

`FileSyncService` 现在支持根据 `SyncDirection` 选择不同的同步策略：

1. **SyncDirectoryInternalAsync**: 根据同步方向分发到具体实现
2. **SyncLocalToRemoteAsync**: 本地到远程同步（原有功能）
3. **SyncRemoteToLocalAsync**: 远程到本地同步（新增功能）

### 远程到本地同步特点

#### 1. 目录处理
- 自动创建本地目录结构
- 递归下载远程文件夹中的所有文件
- 保持远程目录结构

#### 2. 文件下载流程
```csharp
// 1. 获取远程文件列表
var remoteFiles = GetRemoteFiles(client, syncGroup.RemotePath);

// 2. 递归遍历远程目录
GetRemoteFilesRecursive(client, currentPath, files);

// 3. 下载文件并报告进度
client.DownloadFile(remoteFile, fileStream, progress => { ... });
```

#### 3. 进度报告
- 文件级别进度：基于单个文件下载的字节数
- 总体进度：基于已处理文件数/总文件数
- 实时更新 UI 进度条

## 关键技术点

### 1. 远程文件遍历
```csharp
private void GetRemoteFilesRecursive(SftpClient client, string currentPath, List<string> files)
{
    var entries = client.ListDirectory(currentPath);
    foreach (var entry in entries)
    {
        if (entry.IsDirectory)
            GetRemoteFilesRecursive(client, fullPath, files);
        else if (entry.IsRegularFile)
            files.Add(fullPath);
    }
}
```

### 2. 路径处理
- 远程路径：使用 Unix 风格路径分隔符 `/`
- 本地路径：使用 Windows 风格路径分隔符 `\`
- 路径转换：`relativePath.Replace('/', '\\')`

### 3. 错误处理
- 远程目录不存在检查
- 网络连接异常处理
- 文件权限错误处理
- 磁盘空间不足处理

## 配置示例

```csharp
var syncGroup = new SyncGroupConfiguration
{
    Name = "配置下载",
    RemotePath = "/home/user/configs",
    LocalPath = @"D:\Downloaded\Configs",
    Direction = SyncDirection.RemoteToLocal,
    Enabled = true
};
```

## 日志输出

```
[FileSync] 开始同步目录 配置下载: /home/user/configs -> D:\Downloaded\Configs
[FileSync] 正在连接到 192.168.1.100:22
[FileSync] 找到 15 个文件需要下载
[FileSync] 正在下载文件: /home/user/configs/app.conf -> D:\Downloaded\Configs\app.conf
[FileSync] 远程到本地同步完成: 配置下载
```

## 注意事项

1. **文件覆盖**：下载时会覆盖现有本地文件
2. **目录权限**：确保本地目录有写入权限
3. **网络稳定性**：大文件下载时注意网络连接稳定性
4. **字符编码**：文件名包含特殊字符时的处理

## 测试建议

1. 测试空目录同步
2. 测试深层嵌套目录结构
3. 测试大文件下载
4. 测试网络中断恢复
5. 测试中文文件名处理

## 未来优化方向

1. **增量同步**：只下载修改过的文件
2. **断点续传**：支持大文件的断点续传
3. **并发下载**：多文件并发下载提升速度
4. **文件校验**：下载后的文件完整性校验
