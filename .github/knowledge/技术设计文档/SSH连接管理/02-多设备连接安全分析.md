# SSH 多设备连接安全性分析与最佳实践

作为SSH连接专家，本文档详细分析DotNetCampus Terminal多设备连接的安全性考虑，包括密钥管理策略、配置文件调整和一键部署功能的安全性要求。

## 1. 多设备密钥管理策略分析

### 共用密钥 vs 独立密钥的安全性对比

#### 方案一：共用一个私钥（不推荐）
```
本机 (私钥: id_rsa)
├── 设备A (公钥: authorized_keys)
├── 设备B (公钥: authorized_keys)  
├── 设备C (公钥: authorized_keys)
└── 设备D (公钥: authorized_keys)
```

**安全风险**：
- **单点失效**：私钥泄露后，所有设备都面临风险
- **权限管理困难**：无法针对单个设备撤销访问权限
- **审计追踪不足**：无法区分来自不同设备的连接
- **密钥轮换复杂**：更换密钥需要更新所有设备

#### 方案二：每设备独立密钥（理论最佳，但复杂）
```
本机
├── device_a_key (私钥) → 设备A (公钥)
├── device_b_key (私钥) → 设备B (公钥)
├── device_c_key (私钥) → 设备C (公钥)
└── device_d_key (私钥) → 设备D (公钥)
```

**安全优势**：
- **隔离性**：单个设备密钥泄露不影响其他设备
- **精细权限控制**：可以为不同设备配置不同权限
- **易于管理**：可以独立撤销、轮换特定设备的密钥
- **审计友好**：通过密钥可以追踪特定设备的操作

#### 方案三：全局SSH密钥复用（第一期实际采用方案）
```
本机全局SSH密钥
├── ~/.ssh/id_ed25519 (私钥) 
├── ~/.ssh/id_ed25519.pub (公钥)
└── 所有设备共用此公钥
    ├── 设备A (authorized_keys)
    ├── 设备B (authorized_keys)
    ├── 设备C (authorized_keys)
    └── 设备D (authorized_keys)
```

**实用优势**：
- **通用性**：用户可以使用任何SSH工具无密码连接所有设备
- **简单性**：只需要管理一个密钥对
- **兼容性**：与用户现有SSH工作流完全兼容
- **用户友好**：提升整体SSH使用体验，而不仅仅是本软件

### 推荐的密钥管理策略（第一期实现）

#### 1. 全局SSH密钥检测和使用
```bash
# 优先检查标准命名的密钥文件
~/.ssh/id_ed25519      # Ed25519 (优先)
~/.ssh/id_rsa          # RSA (备选)
~/.ssh/id_ecdsa        # ECDSA (备选)
~/.ssh/id_dsa          # DSA (已废弃)

# 如果标准命名不存在，扫描其他可能的命名方式
~/.ssh/ssh_host_rsa_key     # 某些工具生成的命名
~/.ssh/github_rsa           # GitHub专用密钥
~/.ssh/default              # 默认命名
~/.ssh/key                  # 简单命名
~/.ssh/private_key          # 描述性命名

# 智能检测策略
1. 按优先级查找已知命名的密钥
2. 扫描.ssh目录下所有无扩展名文件
3. 验证文件头格式（-----BEGIN ... PRIVATE KEY-----）
4. 尝试用SSH.NET加载验证
```

#### 2. 密钥生成策略（如果不存在）
```bash
# 生成全局Ed25519密钥对
ssh-keygen -t ed25519 -f ~/.ssh/id_ed25519 -C "$(whoami)@$(hostname)-$(date +%Y%m%d)"
```

#### 3. 一键部署流程设计
```
1. 检测本机是否存在全局SSH私钥
   ├── 存在 → 使用现有密钥对
   └── 不存在 → 生成新的全局密钥对
2. 连接到目标设备（使用密码）
3. 将公钥部署到远程设备的 ~/.ssh/authorized_keys
4. 验证密钥认证是否正常工作
5. 可选：提示用户是否禁用密码认证（提升安全性）
6. 更新本地配置为密钥认证模式
```

## 2. 配置文件安全性调整方案

### 当前配置结构分析
```toml
[[SshDevices]]
LocalId = "device_a1b2c3d4e5f67890"
ConnectionName = "麒麟 Kylin (x86_64)"
Host = "172.20.114.71"
Port = 22
UserName = "seewo"
Password = "123"  # 明文密码 - 安全风险
```

### 支持全局SSH密钥的配置升级方案

#### 方案一：简化配置（推荐用于第一期）
```toml
[[SshDevices]]
LocalId = "device_a1b2c3d4e5f67890"
ConnectionName = "麒麟 Kylin (x86_64)"
Host = "172.20.114.71"
Port = 22
UserName = "seewo"
AuthType = "Key"  # "Password" | "Key" | "Auto"
# PrivateKeyPath 留空表示使用系统默认全局密钥
PrivateKeyPath = ""  # 空值 = 自动检测各种可能的密钥文件
PassphraseRequired = false  # 标记全局密钥是否需要密码短语
```

#### 方案二：显式指定全局密钥路径
```toml
[[SshDevices]]
LocalId = "device_a1b2c3d4e5f67890"
ConnectionName = "麒麟 Kylin (x86_64)"
Host = "172.20.114.71"
Port = 22
UserName = "seewo"
AuthType = "Key"
PrivateKeyPath = "~/.ssh/id_ed25519"  # 显式指定全局密钥
```

#### 方案三：混合认证（兼容性最好）
```toml
[[SshDevices]]
LocalId = "device_a1b2c3d4e5f67890"
ConnectionName = "麒麟 Kylin (x86_64)"
Host = "172.20.114.71"
Port = 22
UserName = "seewo"
AuthType = "Auto"  # 自动尝试：密钥优先，失败时使用密码
PrivateKeyPath = ""  # 空值 = 使用全局默认密钥
# Password字段在运行时输入，不存储
```

### 配置文件安全性改进

#### 1. 敏感信息处理
```toml
# 移除明文密码
# Password = "123"  # 删除此行

# 添加安全存储引用
PasswordStorageKey = "DotNetCampus.Terminal.Device.{LocalId}.Password"
PassphraseStorageKey = "DotNetCampus.Terminal.Device.{LocalId}.Passphrase"
```

#### 2. 配置文件权限
- Windows: 限制文件访问权限为当前用户
- Linux/macOS: `chmod 600 terminal.toml`

#### 3. 配置加密（可选）
```toml
# 配置文件头部添加加密标识
[Encryption]
Enabled = true
Algorithm = "AES-256-GCM"
# 密钥通过用户密码或Windows DPAPI派生
```

## 3. 一键部署功能安全性考虑（全局密钥方案）

### 一键部署流程设计
```
1. 检测本机全局SSH密钥（~/.ssh/id_ed25519, ~/.ssh/id_rsa 等）
   ├── 存在 → 使用现有全局密钥对
   └── 不存在 → 生成新的全局密钥对（id_ed25519）
2. 使用SSH密码连接到目标设备
3. 将全局公钥部署到远程设备的 ~/.ssh/authorized_keys
4. 验证密钥认证是否正常工作
5. 可选：提示用户是否禁用设备密码认证
6. 更新本地配置为密钥认证模式（AuthType = "Key", PrivateKeyPath = ""）
```

### 全局密钥方案的安全性考虑

#### 1. 密钥检测和生成安全性
```csharp
// 检测现有密钥时的安全验证
public static bool ValidatePrivateKey(string keyPath)
{
    try
    {
        // 验证密钥文件格式和权限
        var keyFile = new PrivateKeyFile(keyPath);
        
        // 检查文件权限（Windows/Linux）
        var fileInfo = new FileInfo(keyPath);
        // TODO: 添加权限检查逻辑
        
        return true;
    }
    catch (Exception ex)
    {
        Log.Warning($"[SSH] 密钥验证失败: {keyPath}, 错误: {ex.Message}");
        return false;
    }
}
```

**风险控制**：
- 验证现有密钥文件的完整性和格式
- 检查密钥文件权限设置
- 生成新密钥时使用强密码学算法（Ed25519）

#### 2. 公钥去重和完整性保护
```bash
# 部署时的去重逻辑（防止重复添加）
grep -F "${public_key_content}" ~/.ssh/authorized_keys || echo "${public_key_content}" >> ~/.ssh/authorized_keys

# 或者使用更安全的方式
sort ~/.ssh/authorized_keys | uniq > ~/.ssh/authorized_keys.tmp && mv ~/.ssh/authorized_keys.tmp ~/.ssh/authorized_keys
```

**风险控制**：
- 防止重复部署相同公钥
- 保护existing authorized_keys内容
- 验证部署后文件完整性

#### 2. 传输过程安全性
```csharp
// 公钥传输使用已建立的SSH连接
using var sftpClient = new SftpClient(connectionInfo);
sftpClient.Connect();

// 验证远程路径安全性
var remoteSshDir = "~/.ssh";
if (!sftpClient.Exists(remoteSshDir))
{
    sftpClient.CreateDirectory(remoteSshDir);
    // 设置正确权限
    sftpClient.ChangePermissions(remoteSshDir, 700);
}
```

**风险控制**：
- 验证远程目录权限
- 防止路径遍历攻击
- 确保authorized_keys文件完整性

#### 3. 全局密钥的权限验证安全性
```csharp
// 验证全局密钥认证
public async Task<bool> ValidateGlobalKeyAuthAsync(SshDeviceConfig device)
{
    var privateKeyPath = GlobalSshKeyManager.FindExistingPrivateKey();
    if (privateKeyPath == null)
    {
        throw new InvalidOperationException("未找到全局SSH私钥");
    }
    
    var keyFile = new PrivateKeyFile(privateKeyPath);
    var connectionInfo = new ConnectionInfo(device.Host, device.Port, device.UserName, 
        new PrivateKeyAuthenticationMethod(device.UserName, keyFile));
    
    using var testClient = new SshClient(connectionInfo);
    try
    {
        testClient.Connect();
        
        // 执行基本命令验证权限
        var result = testClient.RunCommand("whoami");
        var isValid = result.Result.Trim() == device.UserName && result.ExitStatus == 0;
        
        Log.Info($"[SSH] 全局密钥认证验证: {(isValid ? "成功" : "失败")}");
        return isValid;
    }
    catch (Exception ex)
    {
        Log.Error($"[SSH] 全局密钥认证验证失败: {ex.Message}");
        return false;
    }
    finally
    {
        testClient.Disconnect();
    }
}
```

**风险控制**：
- 部署后立即验证全局密钥认证
- 验证失败时回滚到密码认证
- 记录详细的验证日志

#### 4. 原密码处理安全性
```csharp
// 使用SecureString处理密码
public class SecurePasswordHandler
{
    private readonly SecureString _password;
    
    public SecurePasswordHandler(string password)
    {
        _password = new SecureString();
        foreach (char c in password)
        {
            _password.AppendChar(c);
        }
        _password.MakeReadOnly();
    }
    
    public void Clear()
    {
        _password?.Dispose();
    }
}
```

**风险控制**：
- 密码在内存中的存在时间最小化
- 使用SecureString或类似安全容器
- 操作完成后立即清理敏感信息

#### 5. 配置更新安全性
```csharp
// 原子性配置更新
public async Task UpdateConfigurationAsync(DeviceConfig newConfig)
{
    var tempFile = Path.GetTempFileName();
    try
    {
        // 写入临时文件
        await WriteConfigToFileAsync(tempFile, newConfig);
        
        // 验证配置完整性
        ValidateConfiguration(tempFile);
        
        // 原子性替换
        File.Replace(tempFile, _configPath, _configPath + ".backup");
    }
    finally
    {
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }
    }
}
```

**风险控制**：
- 配置文件原子性更新
- 更新失败时自动回滚
- 保留配置备份文件

### 一键部署UI安全性设计

#### 1. 用户确认机制
```xml
<!-- 多步骤确认对话框 -->
<StackPanel>
    <TextBlock>此操作将：</TextBlock>
    <TextBlock>1. 生成新的SSH密钥对</TextBlock>
    <TextBlock>2. 将公钥部署到远程设备</TextBlock>
    <TextBlock>3. 验证密钥认证</TextBlock>
    <TextBlock Foreground="Orange">4. 可选：禁用设备密码认证</TextBlock>
    
    <CheckBox x:Name="DisablePasswordAuth">
        禁用密码认证（提高安全性，但需确保密钥正常工作）
    </CheckBox>
    <CheckBox x:Name="ConfirmOperation" IsChecked="False">
        我理解此操作的安全影响并同意继续
    </CheckBox>
</StackPanel>
```

#### 2. 进度反馈和错误处理
```csharp
public class KeyDeploymentProgress
{
    public string CurrentStep { get; set; }
    public int PercentComplete { get; set; }
    public bool HasError { get; set; }
    public string ErrorMessage { get; set; }
    public bool CanRetry { get; set; }
    public bool CanRollback { get; set; }
}
```

## 4. 全局密钥方案安全性检查清单

### 部署前检查
- [ ] 检测现有全局SSH密钥（~/.ssh/id_ed25519, ~/.ssh/id_rsa等）
- [ ] 验证现有密钥文件完整性和权限
- [ ] 确认目标设备SSH服务配置正常
- [ ] 验证用户权限（是否可以修改~/.ssh/authorized_keys）
- [ ] 检查网络连接安全性

### 部署过程检查
- [ ] 全局密钥生成使用强算法（Ed25519优先）
- [ ] 公钥传输过程完整性验证
- [ ] 远程authorized_keys文件去重处理
- [ ] 远程文件权限设置正确（700 for .ssh, 600 for authorized_keys）
- [ ] 全局密钥认证验证成功

### 部署后检查
- [ ] 全局密钥可以正常认证所有已配置设备
- [ ] 原密码信息已安全清理（可选）
- [ ] 配置文件已更新为密钥认证模式
- [ ] 审计日志已记录部署操作
- [ ] 用户已收到成功通知和使用指导

### 用户体验检查
- [ ] 用户可以使用任何SSH工具无密码连接
- [ ] 现有SSH工作流程未受影响
- [ ] 密钥轮换和管理流程清晰
- [ ] 提供密钥管理的用户指导文档

## 6. 全局密钥方案的优缺点总结

### 优点
- **用户友好**：提升整体SSH使用体验，不仅限于本软件
- **简单易用**：只需管理一个全局密钥对
- **通用兼容**：与所有SSH工具和现有工作流兼容
- **降低门槛**：用户无需理解复杂的密钥管理概念

### 缺点和风险
- **单点风险**：全局密钥泄露影响所有配置的设备
- **权限粗糙**：无法为不同设备设置不同权限
- **审计困难**：难以区分来自不同设备管理器的连接

### 风险缓解措施
- **定期轮换**：建议每6-12个月轮换全局密钥
- **访问监控**：在关键设备上启用SSH访问日志
- **权限最小化**：确保SSH用户只有必要的权限
- **备份策略**：安全备份全局私钥，防止丢失

## 相关文档

- [SSH 密钥认证配置指南](SSH-Key-Based-Authentication-Guide.md)
- [Terminal TOML 配置设计](Terminal-TOML-Configuration-Design.md)
- [SSH.NET 使用指南](SSH.NET-使用指南.md)

---

**维护说明**：本文档由SSH连接专家维护，涵盖多设备连接的安全性考虑和最佳实践。其他AI在开发相关功能时应严格遵循这些安全准则。

## 5. 全局SSH密钥管理实现指南

### SSH密钥检测逻辑
```csharp
public class GlobalSshKeyManager
{
    // 扩展密钥检测列表，覆盖更多常见的命名方式
    private static readonly string[] KeyPriority = {
        // 标准命名
        "id_ed25519",     // Ed25519 (现代、安全)
        "id_rsa",         // RSA (广泛兼容)
        "id_ecdsa",       // ECDSA (椭圆曲线)
        "id_dsa",         // DSA (已废弃，但可能存在)
        
        // 其他常见命名方式
        "ssh_host_rsa_key",     // 某些工具生成的命名
        "ssh_host_ed25519_key", // 某些工具生成的命名
        "github_rsa",           // GitHub特定密钥
        "gitlab_rsa",           // GitLab特定密钥
        "default",              // 一些SSH客户端的默认命名
        "key",                  // 简单命名
        "private_key",          // 描述性命名
        "ssh_private_key",      // 描述性命名
    };
    
    public static string? FindExistingPrivateKey()
    {
        var sshDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
        
        if (!Directory.Exists(sshDir))
        {
            Log.Info("[SSH] .ssh 目录不存在");
            return null;
        }
        
        // 首先按优先级查找已知命名的密钥
        foreach (var keyName in KeyPriority)
        {
            var keyPath = Path.Combine(sshDir, keyName);
            if (File.Exists(keyPath) && IsValidPrivateKey(keyPath))
            {
                Log.Info($"[SSH] 发现现有私钥: {keyPath}");
                return keyPath;
            }
        }
        
        // 如果没找到，扫描所有文件，查找可能的私钥文件
        var allFiles = Directory.GetFiles(sshDir, "*", SearchOption.TopDirectoryOnly)
            .Where(f => !Path.GetFileName(f).EndsWith(".pub")) // 排除公钥文件
            .Where(f => !Path.GetFileName(f).StartsWith("known_hosts")) // 排除known_hosts
            .Where(f => !Path.GetFileName(f).StartsWith("config")) // 排除config文件
            .Where(f => !Path.GetFileName(f).Contains(".")) // 排除有扩展名的文件
            .ToArray();
        
        foreach (var filePath in allFiles)
        {
            if (IsValidPrivateKey(filePath))
            {
                Log.Info($"[SSH] 发现潜在私钥文件: {filePath}");
                return filePath;
            }
        }
        
        Log.Info("[SSH] 未发现任何SSH私钥");
        return null;
    }
    
    private static bool IsValidPrivateKey(string keyPath)
    {
        try
        {
            // 检查文件是否存在且可读
            if (!File.Exists(keyPath))
                return false;
            
            // 检查文件大小（私钥通常不会太小）
            var fileInfo = new FileInfo(keyPath);
            if (fileInfo.Length < 100) // 私钥至少应该有100字节
                return false;
            
            // 读取文件开头，检查是否是SSH私钥格式
            var firstLine = File.ReadLines(keyPath).FirstOrDefault()?.Trim();
            if (string.IsNullOrEmpty(firstLine))
                return false;
            
            // 检查常见的私钥文件头
            var validHeaders = new[]
            {
                "-----BEGIN OPENSSH PRIVATE KEY-----",  // OpenSSH 新格式
                "-----BEGIN RSA PRIVATE KEY-----",      // RSA 私钥
                "-----BEGIN EC PRIVATE KEY-----",       // ECDSA 私钥
                "-----BEGIN DSA PRIVATE KEY-----",      // DSA 私钥
                "-----BEGIN PRIVATE KEY-----",          // PKCS#8 格式
                "-----BEGIN ENCRYPTED PRIVATE KEY-----" // 加密的私钥
            };
            
            if (validHeaders.Any(header => firstLine.StartsWith(header)))
            {
                // 进一步验证：尝试用SSH.NET加载
                try
                {
                    var keyFile = new PrivateKeyFile(keyPath);
                    return true;
                }
                catch
                {
                    // 如果SSH.NET无法加载，可能需要密码短语，但仍然是有效的私钥文件
                    Log.Warning($"[SSH] 私钥文件可能需要密码短语: {keyPath}");
                    return true; // 仍然认为是有效的，但需要用户输入密码短语
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Log.Warning($"[SSH] 验证私钥文件时出错: {keyPath}, 错误: {ex.Message}");
            return false;
        }
    }
    
    public static string GetPublicKeyContent(string privateKeyPath)
    {
        // 尝试多种公钥文件命名方式
        var possiblePublicKeyPaths = new[]
        {
            privateKeyPath + ".pub",                    // 标准命名
            privateKeyPath.Replace("_key", "_key.pub"), // 某些工具的命名习惯
            Path.ChangeExtension(privateKeyPath, ".pub") // 直接替换扩展名
        };
        
        foreach (var publicKeyPath in possiblePublicKeyPaths)
        {
            if (File.Exists(publicKeyPath))
            {
                var content = File.ReadAllText(publicKeyPath).Trim();
                if (!string.IsNullOrEmpty(content) && (content.StartsWith("ssh-") || content.StartsWith("ecdsa-") || content.StartsWith("ssh-ed25519")))
                {
                    Log.Info($"[SSH] 找到对应公钥文件: {publicKeyPath}");
                    return content;
                }
            }
        }
        
        // 如果找不到公钥文件，尝试从私钥生成
        try
        {
            Log.Info($"[SSH] 未找到公钥文件，尝试从私钥生成: {privateKeyPath}");
            var keyFile = new PrivateKeyFile(privateKeyPath);
            // 注意：SSH.NET 可能不直接支持导出公钥内容，这里需要其他方式
            // 可以通过ssh-keygen命令生成
            return GeneratePublicKeyFromPrivateKey(privateKeyPath);
        }
        catch (Exception ex)
        {
            throw new FileNotFoundException($"无法找到或生成公钥文件，私钥: {privateKeyPath}, 错误: {ex.Message}");
        }
    }
    
    private static string GeneratePublicKeyFromPrivateKey(string privateKeyPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ssh-keygen",
                Arguments = $"-y -f \"{privateKeyPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            using var process = Process.Start(startInfo);
            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                var publicKey = process.StandardOutput.ReadToEnd().Trim();
                if (!string.IsNullOrEmpty(publicKey))
                {
                    Log.Info($"[SSH] 成功从私钥生成公钥内容");
                    return publicKey;
                }
            }
            
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"ssh-keygen 生成公钥失败: {error}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"从私钥生成公钥失败: {ex.Message}");
        }
    }
}
```

### 密钥对生成逻辑
```csharp
public class SshKeyGenerator
{
    public static async Task<string> GenerateGlobalKeyPairAsync()
    {
        var sshDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
        Directory.CreateDirectory(sshDir);
        
        var keyPath = Path.Combine(sshDir, "id_ed25519");
        
        if (File.Exists(keyPath))
        {
            Log.Info($"[SSH] 全局密钥已存在: {keyPath}");
            return keyPath;
        }
        
        var comment = $"{Environment.UserName}@{Environment.MachineName}-{DateTime.Now:yyyyMMdd}";
        
        // 使用 ssh-keygen 生成密钥
        var startInfo = new ProcessStartInfo
        {
            FileName = "ssh-keygen",
            Arguments = $"-t ed25519 -f \"{keyPath}\" -C \"{comment}\" -N \"\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        using var process = Process.Start(startInfo);
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"密钥生成失败: {error}");
        }
        
        Log.Info($"[SSH] 成功生成全局密钥对: {keyPath}");
        return keyPath;
    }
}
```

### 一键部署完整实现
```csharp
public class SshKeyDeploymentService
{
    public async Task<bool> DeployKeyToDeviceAsync(SshDeviceConfig device, string password, string? passphrase = null)
    {
        try
        {
            // 1. 检测或生成全局密钥
            var privateKeyPath = GlobalSshKeyManager.FindExistingPrivateKey() 
                ?? await SshKeyGenerator.GenerateGlobalKeyPairAsync();
            
            var publicKeyContent = GlobalSshKeyManager.GetPublicKeyContent(privateKeyPath);
            
            // 2. 使用密码连接到设备
            var passwordAuth = new PasswordAuthenticationMethod(device.UserName, password);
            var connectionInfo = new ConnectionInfo(device.Host, device.Port, device.UserName, passwordAuth);
            
            using var client = new SshClient(connectionInfo);
            client.Connect();
            
            // 3. 部署公钥到远程设备
            await DeployPublicKeyAsync(client, publicKeyContent);
            
            // 4. 验证密钥认证
            PrivateKeyFile keyFile;
            if (!string.IsNullOrEmpty(passphrase))
            {
                keyFile = new PrivateKeyFile(privateKeyPath, passphrase);
            }
            else
            {
                try
                {
                    keyFile = new PrivateKeyFile(privateKeyPath);
                }
                catch (Exception ex) when (ex.Message.Contains("passphrase") || ex.Message.Contains("encrypted"))
                {
                    throw new InvalidOperationException("私钥需要密码短语，请提供passphrase参数");
                }
            }
            
            var keyAuth = new PrivateKeyAuthenticationMethod(device.UserName, keyFile);
            var testConnection = new ConnectionInfo(device.Host, device.Port, device.UserName, keyAuth);
            
            using var testClient = new SshClient(testConnection);
            testClient.Connect();
            
            var whoami = testClient.RunCommand("whoami");
            if (whoami.Result.Trim() != device.UserName)
            {
                throw new SecurityException("密钥认证验证失败");
            }
            
            // 5. 更新配置文件
            device.AuthType = "Key";
            device.PrivateKeyPath = "";  // 空值表示使用全局默认密钥
            device.PassphraseRequired = !string.IsNullOrEmpty(passphrase); // 记录是否需要密码短语
            
            Log.Info($"[SSH] 成功为设备 {device.ConnectionName} 部署SSH密钥");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[SSH] 密钥部署失败: {ex.Message}");
            return false;
        }
    }
    
    private async Task DeployPublicKeyAsync(SshClient client, string publicKeyContent)
    {
        var commands = new []
        {
            "mkdir -p ~/.ssh",
            "chmod 700 ~/.ssh",
            $"echo '{publicKeyContent}' >> ~/.ssh/authorized_keys",
            "chmod 600 ~/.ssh/authorized_keys",
            // 去重：移除重复的公钥条目
            "sort ~/.ssh/authorized_keys | uniq > ~/.ssh/authorized_keys.tmp && mv ~/.ssh/authorized_keys.tmp ~/.ssh/authorized_keys"
        };
        
        foreach (var command in commands)
        {
            var result = client.RunCommand(command);
            if (result.ExitStatus != 0)
            {
                throw new InvalidOperationException($"命令执行失败: {command}, 错误: {result.Error}");
            }
        }
    }
}
```

### 配置文件更新逻辑
```csharp
public class ConfigurationUpdater
{
    public void UpdateDeviceToKeyAuth(SshDeviceConfig device)
    {
        // 更新为密钥认证
        device.AuthType = "Key";
        device.PrivateKeyPath = "";  // 空值 = 使用全局默认密钥
        
        // 清除密码（出于安全考虑）
        device.Password = null;
        
        Log.Info($"[Config] 设备 {device.ConnectionName} 已更新为密钥认证模式");
    }
    
    public void UpdateDeviceToAutoAuth(SshDeviceConfig device)
    {
        // 设置为自动认证（密钥优先，密码备用）
        device.AuthType = "Auto";
        device.PrivateKeyPath = "";  // 使用全局默认密钥
        
        // 保留密码配置作为备用（可选）
        // device.Password 保持不变
        
        Log.Info($"[Config] 设备 {device.ConnectionName} 已更新为自动认证模式");
    }
}
```

## 7. 实际部署中的常见情况处理

### 7.1 密钥文件命名多样性处理

实际用户环境中，SSH密钥文件可能有各种命名方式：

#### Windows 环境常见命名
```
%USERPROFILE%\.ssh\
├── id_rsa              # OpenSSH 标准
├── ssh_host_rsa_key    # Windows OpenSSH Server
├── github_rsa          # GitHub Desktop 生成
├── key                 # 用户自定义
├── private_key         # 描述性命名
└── default             # 某些SSH工具默认命名
```

#### Linux/macOS 环境常见命名
```
~/.ssh/
├── id_ed25519          # 现代标准
├── id_rsa              # 传统标准  
├── id_ecdsa            # ECDSA 算法
├── gitlab_rsa          # GitLab 专用
├── work_key            # 工作用途
└── personal_key        # 个人用途
```

### 7.2 密码短语保护密钥处理

```csharp
public class PassphraseHandling
{
    public static ConnectionInfo CreateConnectionWithKey(SshDeviceConfig device, string? passphrase = null)
    {
        var privateKeyPath = GlobalSshKeyManager.FindExistingPrivateKey();
        if (privateKeyPath == null)
        {
            throw new InvalidOperationException("未找到全局SSH私钥");
        }
        
        PrivateKeyFile keyFile;
        
        // 首先尝试无密码短语加载
        try
        {
            keyFile = new PrivateKeyFile(privateKeyPath);
            Log.Info("[SSH] 成功加载无密码短语保护的私钥");
        }
        catch (Exception ex) when (IsPassphraseRequired(ex))
        {
            if (string.IsNullOrEmpty(passphrase))
            {
                throw new InvalidOperationException("私钥需要密码短语，请提供密码短语");
            }
            
            try
            {
                keyFile = new PrivateKeyFile(privateKeyPath, passphrase);
                Log.Info("[SSH] 成功加载密码短语保护的私钥");
            }
            catch (Exception passphraseEx)
            {
                throw new UnauthorizedAccessException($"密码短语错误或私钥文件损坏: {passphraseEx.Message}");
            }
        }
        
        var authMethod = new PrivateKeyAuthenticationMethod(device.UserName, keyFile);
        return new ConnectionInfo(device.Host, device.Port, device.UserName, authMethod);
    }
    
    private static bool IsPassphraseRequired(Exception ex)
    {
        var message = ex.Message.ToLower();
        return message.Contains("passphrase") || 
               message.Contains("encrypted") || 
               message.Contains("password") ||
               message.Contains("decrypt");
    }
}
```

### 7.3 多种密钥格式支持

现代SSH环境可能包含不同格式的密钥：

```csharp
public static class KeyFormatDetection
{
    public static string DetectKeyFormat(string keyPath)
    {
        try
        {
            var firstLine = File.ReadLines(keyPath).FirstOrDefault()?.Trim();
            
            return firstLine switch
            {
                var line when line.StartsWith("-----BEGIN OPENSSH PRIVATE KEY-----") => "OpenSSH",
                var line when line.StartsWith("-----BEGIN RSA PRIVATE KEY-----") => "RSA (PEM)",
                var line when line.StartsWith("-----BEGIN EC PRIVATE KEY-----") => "ECDSA (PEM)",
                var line when line.StartsWith("-----BEGIN DSA PRIVATE KEY-----") => "DSA (PEM)",
                var line when line.StartsWith("-----BEGIN PRIVATE KEY-----") => "PKCS#8",
                var line when line.StartsWith("-----BEGIN ENCRYPTED PRIVATE KEY-----") => "Encrypted PKCS#8",
                _ => "Unknown"
            };
        }
        catch
        {
            return "Error";
        }
    }
    
    public static bool IsSupportedBySSHNet(string keyPath)
    {
        try
        {
            var keyFile = new PrivateKeyFile(keyPath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

### 7.4 用户友好的错误处理

```csharp
public class UserFriendlyKeyDeployment
{
    public async Task<KeyDeploymentResult> DeployWithUserGuidance(SshDeviceConfig device, string password)
    {
        var result = new KeyDeploymentResult();
        
        try
        {
            // 1. 检测现有密钥
            var privateKeyPath = GlobalSshKeyManager.FindExistingPrivateKey();
            
            if (privateKeyPath == null)
            {
                result.AddStep("未找到现有SSH密钥，将生成新的密钥对...");
                privateKeyPath = await SshKeyGenerator.GenerateGlobalKeyPairAsync();
                result.AddStep($"成功生成新密钥: {Path.GetFileName(privateKeyPath)}");
            }
            else
            {
                var keyFormat = KeyFormatDetection.DetectKeyFormat(privateKeyPath);
                result.AddStep($"找到现有密钥: {Path.GetFileName(privateKeyPath)} ({keyFormat})");
            }
            
            // 2. 检查密钥是否需要密码短语
            bool needsPassphrase = false;
            try
            {
                var testKey = new PrivateKeyFile(privateKeyPath);
            }
            catch (Exception ex) when (IsPassphraseRequired(ex))
            {
                needsPassphrase = true;
                result.AddStep("检测到密钥需要密码短语保护");
            }
            
            // 3. 部署公钥
            result.AddStep("正在连接远程设备...");
            var publicKeyContent = GlobalSshKeyManager.GetPublicKeyContent(privateKeyPath);
            
            var passwordAuth = new PasswordAuthenticationMethod(device.UserName, password);
            var connectionInfo = new ConnectionInfo(device.Host, device.Port, device.UserName, passwordAuth);
            
            using var client = new SshClient(connectionInfo);
            client.Connect();
            result.AddStep("已连接到远程设备");
            
            await DeployPublicKeyAsync(client, publicKeyContent);
            result.AddStep("公钥已部署到远程设备");
            
            // 4. 验证密钥认证
            result.AddStep("正在验证密钥认证...");
            
            PrivateKeyFile keyFile;
            if (needsPassphrase)
            {
                // 这里可能需要UI交互获取密码短语
                result.RequiresPassphrase = true;
                result.PrivateKeyPath = privateKeyPath;
                return result; // 返回给UI处理密码短语输入
            }
            else
            {
                keyFile = new PrivateKeyFile(privateKeyPath);
            }
            
            var keyAuth = new PrivateKeyAuthenticationMethod(device.UserName, keyFile);
            var testConnection = new ConnectionInfo(device.Host, device.Port, device.UserName, keyAuth);
            
            using var testClient = new SshClient(testConnection);
            testClient.Connect();
            
            var whoami = testClient.RunCommand("whoami");
            if (whoami.Result.Trim() == device.UserName)
            {
                result.AddStep("密钥认证验证成功！");
                result.Success = true;
                
                // 更新配置
                device.AuthType = "Key";
                device.PrivateKeyPath = "";
                device.PassphraseRequired = needsPassphrase;
                
                result.AddStep("配置已更新为密钥认证模式");
            }
            else
            {
                throw new SecurityException("密钥认证验证失败");
            }
            
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.AddStep($"部署失败: {ex.Message}");
        }
        
        return result;
    }
    
    private static bool IsPassphraseRequired(Exception ex)
    {
        var message = ex.Message.ToLower();
        return message.Contains("passphrase") || 
               message.Contains("encrypted") || 
               message.Contains("password");
    }
}

public class KeyDeploymentResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Steps { get; set; } = new();
    public bool RequiresPassphrase { get; set; }
    public string? PrivateKeyPath { get; set; }
    
    public void AddStep(string step)
    {
        Steps.Add($"[{DateTime.Now:HH:mm:ss}] {step}");
    }
}
```

### 7.5 配置文件向后兼容性

```toml
# 支持旧版本配置格式
[[SshDevices]]
LocalId = "device_old_format"
ConnectionName = "旧格式设备"
Host = "192.168.1.100"
Port = 22
UserName = "user"
Password = "123"  # 旧格式：明文密码

# 自动升级为新格式后
[[SshDevices]]
LocalId = "device_old_format"
ConnectionName = "旧格式设备"
Host = "192.168.1.100"
Port = 22
UserName = "user"
AuthType = "Auto"  # 新格式：自动认证
PrivateKeyPath = "" # 使用全局密钥
PassphraseRequired = false
# Password = "123"  # 移除明文密码，改为安全存储
```
