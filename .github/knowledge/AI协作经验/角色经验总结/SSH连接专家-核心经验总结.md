# SSH连接专家核心经验总结

## 角色定位
SSH连接专家负责 DotNetCampus Terminal 项目中所有与SSH连接相关的功能开发和维护。

## 核心职责
1. SSH.NET库的封装和连接管理
2. SSH密钥认证配置和部署
3. 多设备连接的安全性分析
4. SSH连接池优化和断线重连机制
5. SSH隧道和端口转发支持

## 技术重点

### SSH.NET 使用要点
- 使用 `SshClient` 进行基本SSH连接
- 使用 `SftpClient` 进行文件传输
- 使用 `PasswordConnectionInfo` 或 `PrivateKeyConnectionInfo` 进行身份认证
- 连接超时设置：默认30秒，可根据网络环境调整
- 异常处理：捕获 `SshException`、`SocketException` 等

### 密钥管理最佳实践
- 支持RSA、ECDSA、Ed25519密钥格式
- 密钥文件路径：`~/.ssh/id_rsa`、`~/.ssh/id_ecdsa`、`~/.ssh/id_ed25519`
- 公钥部署：自动追加到远程 `~/.ssh/authorized_keys`
- 权限设置：私钥600，公钥644，.ssh目录700

### 连接状态管理
- 实现连接池避免频繁建立连接
- 心跳检测机制保持长连接
- 优雅的断线重连策略
- 连接状态通知UI层

## 核心代码模式

### SSH连接封装
```csharp
public class SshConnectionManager : ISshConnectionManager
{
    private readonly ConcurrentDictionary<string, SshClient> _connections = new();
    
    public async Task<SshClient> GetConnectionAsync(SshDeviceInfo deviceInfo)
    {
        var key = deviceInfo.GetConnectionKey();
        return _connections.GetOrAdd(key, _ => CreateConnection(deviceInfo));
    }
    
    private SshClient CreateConnection(SshDeviceInfo deviceInfo)
    {
        var connectionInfo = CreateConnectionInfo(deviceInfo);
        var client = new SshClient(connectionInfo);
        client.Connect();
        return client;
    }
}
```

### 密钥部署流程
```csharp
public async Task<bool> DeployPublicKeyAsync(SshDeviceInfo deviceInfo, string publicKeyPath)
{
    try
    {
        using var sftpClient = new SftpClient(deviceInfo.ToConnectionInfo());
        sftpClient.Connect();
        
        var publicKeyContent = await File.ReadAllTextAsync(publicKeyPath);
        var authorizedKeysPath = ".ssh/authorized_keys";
        
        // 检查是否已存在
        if (!await IsKeyAlreadyDeployedAsync(sftpClient, authorizedKeysPath, publicKeyContent))
        {
            await AppendKeyToAuthorizedKeysAsync(sftpClient, authorizedKeysPath, publicKeyContent);
        }
        
        return true;
    }
    catch (Exception ex)
    {
        Log.Error($"[SSH] 密钥部署失败: {ex.Message}");
        return false;
    }
}
```

## 踩坑经验

### 1. SSH连接超时问题
- **问题**：网络不稳定时连接经常超时
- **解决**：设置合适的超时时间，实现重试机制
- **代码**：`connectionInfo.Timeout = TimeSpan.FromSeconds(30)`

### 2. 密钥格式兼容性
- **问题**：OpenSSH新格式私钥无法直接使用
- **解决**：使用SSH.NET支持的格式，或转换密钥格式
- **经验**：优先推荐RSA 2048位密钥的广泛兼容性

### 3. 并发连接管理
- **问题**：多个操作同时建立连接导致资源浪费
- **解决**：实现连接池，复用已建立的连接
- **注意**：及时释放不用的连接避免资源泄漏

### 4. SFTP路径处理
- **问题**：Windows和Linux路径分隔符不一致
- **解决**：统一使用Unix路径格式，使用Path.Combine谨慎处理
- **代码**：`remotePath = remotePath.Replace('\\', '/')`

## 与其他模块协作

### 与UI界面设计师协作
- 提供连接状态变更事件供UI绑定
- 配合实现密钥部署进度显示
- 提供错误信息格式化供UI展示

### 与文件同步工程师协作
- 提供稳定的SFTP连接用于文件传输
- 配合优化文件同步性能
- 共享连接池减少资源占用

### 与配置管理专家协作
- 定义SSH设备配置数据结构
- 支持密钥路径配置和验证
- 实现配置变更时的连接更新

## 日志规范
- 标签：`[SSH]`
- 连接事件：`Log.Info("[SSH] 连接到设备: {deviceName}")`
- 错误处理：`Log.Error("[SSH] 连接失败: {error}")`
- 密钥操作：`Log.Info("[SSH] 密钥部署成功: {deviceName}")`

## 性能优化要点
1. 连接复用：避免频繁建立新连接
2. 异步操作：所有网络操作使用异步模式
3. 超时控制：合理设置各种操作的超时时间
4. 资源释放：及时关闭不用的连接和流
5. 批量操作：尽可能批量处理多个SSH命令

## 安全考虑
1. 密钥权限：确保私钥文件权限正确
2. 连接加密：使用强加密算法
3. 会话管理：避免长时间保持不活跃连接
4. 错误信息：避免在日志中暴露敏感信息
5. 密钥轮换：支持定期更换SSH密钥对
