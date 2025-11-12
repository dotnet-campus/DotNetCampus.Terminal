# SSH.NET 文件同步使用指南

本文档提供了使用 SSH.NET 进行 SFTP 文件同步的核心知识和最佳实践。

## 1. 基础概念

### SSH.NET 库概述

SSH.NET 是一个基于 .NET 的 SSH 客户端库，提供了丰富的 SSH 功能，包括：

- SSH 命令执行（`SshClient`）
- SFTP 文件传输（`SftpClient`）
- SCP 文件传输
- 端口转发
- 交互式 Shell

### 主要组件

SSH.NET 中的主要类型包括：

- `SshClient` - 用于执行 SSH 命令
- `SftpClient` - 用于 SFTP 文件传输
- `ScpClient` - 用于 SCP 文件传输
- `ForwardedPort` - 用于端口转发
- `ShellStream` - 用于交互式 Shell

## 2. SFTP 文件传输

### 建立连接

```csharp
// 基于用户名密码的连接
using var client = new SftpClient("host", port, "username", "password");
client.Connect();

// 基于私钥的连接
using var client = new SftpClient("host", port, "username", new PrivateKeyFile("path/to/key"));
client.Connect();
```

### 上传文件

```csharp
// 上传文件（覆盖）
using (var fileStream = File.OpenRead("localPath"))
{
    client.UploadFile(fileStream, "remotePath", true);
}

// 带进度的上传
using (var fileStream = File.OpenRead("localPath"))
{
    client.UploadFile(fileStream, "remotePath", true, progress => 
    {
        double percent = (double)progress / fileStream.Length * 100;
        Console.WriteLine($"上传进度: {percent:F2}%");
    });
}
```

### 下载文件

```csharp
// 下载文件
using (var fileStream = File.Create("localPath"))
{
    client.DownloadFile("remotePath", fileStream);
}

// 带进度的下载
using (var fileStream = File.Create("localPath"))
{
    client.DownloadFile("remotePath", fileStream, progress => 
    {
        var fileInfo = client.GetAttributes("remotePath");
        double percent = (double)progress / fileInfo.Size * 100;
        Console.WriteLine($"下载进度: {percent:F2}%");
    });
}
```

### 列出目录

```csharp
// 列出目录内容
var files = client.ListDirectory("/path/to/dir");
foreach (var file in files)
{
    Console.WriteLine($"{file.FullName} - {file.Length} bytes");
}
```

### 文件和目录操作

```csharp
// 创建目录
client.CreateDirectory("/path/to/newDir");

// 检查文件或目录是否存在
bool exists = client.Exists("/path/to/file");

// 删除文件
client.DeleteFile("/path/to/file");

// 删除目录
client.DeleteDirectory("/path/to/dir");

// 重命名文件或目录
client.RenameFile("/path/to/oldName", "/path/to/newName");
```

## 3. 异步操作

虽然 SSH.NET 的 API 主要是同步的，但可以通过 Task.Run 在后台线程上执行：

```csharp
await Task.Run(() =>
{
    using (var client = new SftpClient(host, port, username, password))
    {
        client.Connect();
        client.UploadFile(stream, remotePath);
        client.Disconnect();
    }
});
```

## 4. 错误处理最佳实践

```csharp
try
{
    client.Connect();
    
    if (!client.IsConnected)
    {
        throw new Exception("无法连接到服务器");
    }
    
    // 执行操作...
}
catch (Renci.SshNet.Common.SshConnectionException ex)
{
    Console.WriteLine($"SSH连接错误: {ex.Message}");
}
catch (Renci.SshNet.Common.SftpPermissionDeniedException ex)
{
    Console.WriteLine($"SFTP权限错误: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"发生错误: {ex.Message}");
}
finally
{
    if (client.IsConnected)
    {
        client.Disconnect();
    }
}
```

## 5. 性能优化技巧

### 文件传输缓冲区

默认缓冲区大小可能不是最优的。对于大文件传输，可以考虑调整缓冲区大小：

```csharp
// 设置缓冲区大小为8MB
client.BufferSize = 8 * 1024 * 1024;
```

### 批量操作

对于多文件操作，保持连接打开状态比反复连接和断开效率更高：

```csharp
client.Connect();
try
{
    foreach (var file in files)
    {
        // 上传或下载文件
    }
}
finally
{
    client.Disconnect();
}
```

### 异步和并行

对于多文件传输，可以考虑并行处理，但要注意控制并发度，避免过度消耗资源：

```csharp
var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
await Parallel.ForEachAsync(files, options, async (file, token) =>
{
    using var client = new SftpClient(host, port, username, password);
    await Task.Run(() => 
    {
        client.Connect();
        // 上传或下载文件
        client.Disconnect();
    }, token);
});
```

## 6. 安全性考虑

### 密码处理

避免在代码中硬编码密码，考虑使用：

- 配置文件（加密存储）
- 环境变量
- 密钥库（如Azure Key Vault）
- 私钥认证（而非密码）

### 密钥认证

使用密钥认证通常比密码认证更安全：

```csharp
var pkFile = new PrivateKeyFile("path/to/key", "passphrase");
using var client = new SftpClient(host, port, username, pkFile);
```

### 连接加密

SSH.NET 支持多种加密方法：

- aes128-ctr, aes192-ctr, aes256-ctr
- aes128-gcm@openssh.com, aes256-gcm@openssh.com
- chacha20-poly1305@openssh.com
- aes128-cbc, aes192-cbc, aes256-cbc
- 3des-cbc

在需要时可以配置连接参数来使用特定的加密方法。

## 7. 常见问题解决

### 连接被拒绝

- 检查主机地址和端口是否正确
- 确保防火墙未阻止连接
- 验证服务器上的SSH服务是否运行

### 认证失败

- 检查用户名和密码是否正确
- 验证密钥文件路径和格式
- 确认服务器上的授权配置

### 权限问题

- 检查远程文件/目录的权限
- 确保用户有适当的读写权限
- 尝试使用具有更高权限的用户

### 连接超时

- 增加连接超时时间：`client.ConnectionInfo.Timeout = TimeSpan.FromMinutes(5);`
- 实现重试机制
- 检查网络连接质量

## 8. 实际使用案例

### 增量同步

```csharp
// 获取远程文件列表
var remoteFiles = client.ListDirectory(remotePath)
    .Where(f => !f.IsDirectory)
    .ToDictionary(f => f.FullName, f => f.LastWriteTime);

// 获取本地文件列表
var localFiles = Directory.GetFiles(localPath, "*", SearchOption.AllDirectories)
    .ToDictionary(f => Path.Combine(remotePath, f.Substring(localPath.Length).TrimStart('\\')).Replace('\\', '/'), 
                 f => File.GetLastWriteTime(f));

// 找出需要更新的文件
var filesToUpdate = localFiles
    .Where(lf => !remoteFiles.ContainsKey(lf.Key) || remoteFiles[lf.Key] < lf.Value)
    .Select(lf => lf.Key)
    .ToList();

// 上传需要更新的文件
foreach (var file in filesToUpdate)
{
    var localFile = Path.Combine(localPath, file.Substring(remotePath.Length).TrimStart('/').Replace('/', '\\'));
    using (var fs = File.OpenRead(localFile))
    {
        client.UploadFile(fs, file);
    }
}
```

### 递归创建远程目录

```csharp
public void EnsureRemoteDirectoryExists(SftpClient client, string remoteDirectory)
{
    string[] directories = remoteDirectory.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
    string currentPath = "/";

    foreach (string directory in directories)
    {
        currentPath = Path.Combine(currentPath, directory).Replace('\\', '/');
        
        if (!client.Exists(currentPath))
        {
            client.CreateDirectory(currentPath);
        }
    }
}
```

## 9. 版本兼容性

SSH.NET 支持以下目标框架：

- .NET Framework 4.6.2 及更高版本
- .NET Standard 2.0
- .NET 8.0 及更高版本

在项目中安装 SSH.NET：

```powershell
dotnet add package SSH.NET
```

## 10. 扩展与集成

### 与进度报告集成

```csharp
public async Task UploadWithProgressAsync(string localPath, string remotePath, IProgress<double> progress, CancellationToken cancellationToken)
{
    await Task.Run(() => 
    {
        using var fileStream = File.OpenRead(localPath);
        long fileSize = fileStream.Length;
        
        client.UploadFile(fileStream, remotePath, true, uploadedBytes => 
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            
            double percentage = (double)uploadedBytes / fileSize * 100;
            progress?.Report(percentage);
        });
    }, cancellationToken);
}
```

### 与日志系统集成

```csharp
public async Task SyncDirectoryAsync(string localPath, string remotePath, ILogger logger)
{
    logger.LogInformation("开始同步 {LocalPath} 到 {RemotePath}", localPath, remotePath);
    
    try
    {
        // 执行同步...
        
        logger.LogInformation("同步完成");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "同步过程中发生错误");
        throw;
    }
}
```

## 总结

SSH.NET 提供了强大而灵活的SSH和SFTP功能，适用于各种远程文件操作场景。通过本文档的最佳实践，可以高效、安全地实现文件同步功能，并解决常见的问题。对于DotNetCampus.Terminal项目，我们利用SSH.NET实现了高效的远程文件同步功能，支持进度报告、取消操作和错误处理。
