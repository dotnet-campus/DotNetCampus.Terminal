# SSH.NET 使用指南

SSH.NET 是一个用于SSH连接的 .NET 库。项目中使用版本：`2024.1.0`

## 基本概念

SSH.NET提供了完整的SSH客户端功能，包括：
- SSH连接和认证
- 命令执行
- SFTP文件传输
- 端口转发

## 项目中的应用场景

### 1. 基本SSH连接
[待补充具体示例]

### 2. SFTP文件同步
[待补充具体示例]

### 3. 连接状态监控
[待补充具体示例]

### 4. 批量命令执行
[待补充具体示例]

## 认证方式

### 1. 密码认证
```csharp
var connectionInfo = new ConnectionInfo("hostname", "username", 
    new PasswordAuthenticationMethod("username", "password"));

using var client = new SshClient(connectionInfo);
client.Connect();
```

### 2. 私钥认证
```csharp
// 无密码短语的私钥
var keyFile = new PrivateKeyFile(@"C:\Users\YourUser\.ssh\id_rsa");
var connectionInfo = new ConnectionInfo("hostname", "username", 
    new PrivateKeyAuthenticationMethod("username", keyFile));

// 有密码短语的私钥
var keyFileWithPassphrase = new PrivateKeyFile(@"C:\Users\YourUser\.ssh\id_rsa", "passphrase");
var connectionInfoSecure = new ConnectionInfo("hostname", "username", 
    new PrivateKeyAuthenticationMethod("username", keyFileWithPassphrase));
```

### 3. 组合认证（推荐）
```csharp
// 优先尝试密钥认证，失败时回退到密码认证
var connectionInfo = new ConnectionInfo("hostname", "username", 
    new PrivateKeyAuthenticationMethod("username", keyFile),
    new PasswordAuthenticationMethod("username", "password"));
```

### 4. 全局SSH密钥认证（推荐）
```csharp
// 自动检测和使用全局SSH密钥
public static ConnectionInfo CreateGlobalKeyConnection(string hostname, int port, string username)
{
    var globalKeyPath = FindGlobalPrivateKey();
    if (globalKeyPath != null)
    {
        var keyFile = new PrivateKeyFile(globalKeyPath);
        return new ConnectionInfo(hostname, port, username, 
            new PrivateKeyAuthenticationMethod(username, keyFile));
    }
    
    // 如果没有全局密钥，回退到密码认证
    throw new InvalidOperationException("未找到全局SSH私钥，请先配置SSH密钥或使用密码认证");
}

private static string? FindGlobalPrivateKey()
{
    var sshDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
    var keyNames = new[] { "id_ed25519", "id_rsa", "id_ecdsa" };
    
    foreach (var keyName in keyNames)
    {
        var keyPath = Path.Combine(sshDir, keyName);
        if (File.Exists(keyPath))
        {
            return keyPath;
        }
    }
    
    return null;
}
```

## 错误处理

### 1. 连接失败处理
[待补充示例]

### 2. 认证失败处理
[待补充示例]

### 3. 网络中断处理
[待补充示例]

## 性能优化

### 1. 连接池管理
[待补充示例]

### 2. 并发连接控制
[待补充示例]

### 3. 超时配置
[待补充示例]

## 最佳实践

[由各AI在实际使用过程中补充]

## 常见问题

[待补充常见问题和解决方案]

---

**注意**: 这是一个框架文档，请各位AI在使用SSH.NET的过程中，将实际经验、示例代码、遇到的问题和解决方案补充到这个文档中。
