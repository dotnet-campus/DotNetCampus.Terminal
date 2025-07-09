# SSH 密钥认证配置指南

作为SSH连接专家，本文档详细说明了如何从密码认证迁移到更安全的私钥认证方式。

## 概述

SSH密钥认证是一种比密码认证更安全、更便捷的认证方式。一旦配置完成，客户端可以自动登录到远程服务器，无需每次输入密码。

## 完整配置流程

### 1. 生成SSH密钥对

在客户端（Windows/Linux）上生成密钥对：

```bash
# 使用 RSA 算法生成密钥对（推荐 4096 位）
ssh-keygen -t rsa -b 4096 -C "your_email@example.com"

# 或使用更现代的 Ed25519 算法（推荐）
ssh-keygen -t ed25519 -C "your_email@example.com"
```

**密钥生成过程中的选择**：
- 密钥保存路径：默认 `~/.ssh/id_rsa`（或 `id_ed25519`）
- 密码短语（passphrase）：可选，增加额外安全性
- 生成两个文件：
  - 私钥：`id_rsa`（保留在客户端，绝不外传）
  - 公钥：`id_rsa.pub`（需要复制到服务器）

### 2. 将公钥复制到远程服务器

#### 方法一：使用 ssh-copy-id（推荐）
```bash
ssh-copy-id username@remote_host
```

#### 方法二：手动复制（适用于Windows或ssh-copy-id不可用的情况）

**步骤 2.1：读取公钥内容**
```bash
# Linux/macOS
cat ~/.ssh/id_rsa.pub

# Windows PowerShell
Get-Content $env:USERPROFILE\.ssh\id_rsa.pub
```

**步骤 2.2：通过SSH连接到远程服务器**
```bash
ssh username@remote_host
```

**步骤 2.3：在远程服务器上配置公钥**
```bash
# 创建 .ssh 目录（如果不存在）
mkdir -p ~/.ssh

# 设置正确的权限
chmod 700 ~/.ssh

# 将公钥内容追加到 authorized_keys 文件
echo "your_public_key_content_here" >> ~/.ssh/authorized_keys

# 设置 authorized_keys 文件权限
chmod 600 ~/.ssh/authorized_keys
```

### 3. 验证密钥认证

退出SSH连接，重新连接验证：
```bash
ssh username@remote_host
```

如果配置正确，应该无需输入密码即可登录。

### 4. 禁用密码认证（可选，增强安全性）

在远程服务器上编辑SSH配置：
```bash
sudo nano /etc/ssh/sshd_config
```

修改以下配置项：
```
PasswordAuthentication no
ChallengeResponseAuthentication no
UsePAM no
```

重启SSH服务：
```bash
sudo systemctl restart sshd
```

## SSH.NET 中的密钥认证实现

### 基本代码示例

```csharp
using Renci.SshNet;

// 使用私钥文件认证
var keyFile = new PrivateKeyFile(@"C:\Users\YourUser\.ssh\id_rsa");
var connectionInfo = new ConnectionInfo("hostname", "username", 
    new PrivateKeyAuthenticationMethod("username", keyFile));

using var client = new SshClient(connectionInfo);
client.Connect();

// 执行命令或SFTP操作
```

### 带密码短语的私钥

```csharp
// 如果私钥有密码短语保护
var keyFile = new PrivateKeyFile(@"C:\Users\YourUser\.ssh\id_rsa", "passphrase");
var connectionInfo = new ConnectionInfo("hostname", "username", 
    new PrivateKeyAuthenticationMethod("username", keyFile));
```

### 多种认证方式组合

```csharp
// 组合多种认证方式，SSH.NET会按顺序尝试
var connectionInfo = new ConnectionInfo("hostname", "username", 
    new PrivateKeyAuthenticationMethod("username", keyFile),
    new PasswordAuthenticationMethod("username", "password"));
```

## 安全最佳实践

### 1. 密钥管理
- **私钥安全**：私钥文件绝不能泄露，建议设置密码短语
- **定期轮换**：定期更换密钥对，特别是在人员变动时
- **备份策略**：安全备份私钥，防止丢失

### 2. 服务器配置
- **禁用root登录**：`PermitRootLogin no`
- **限制用户**：`AllowUsers username1 username2`
- **更改默认端口**：避免使用22端口
- **启用防火墙**：只开放必要的SSH端口

### 3. 文件权限
- 私钥文件：`600` 或 `400`
- 公钥文件：`644`
- `.ssh` 目录：`700`
- `authorized_keys`：`600`

## 常见问题和解决方案

### 1. 权限错误
**问题**：SSH仍然要求密码
**解决方案**：检查文件权限
```bash
# 修复权限
chmod 700 ~/.ssh
chmod 600 ~/.ssh/authorized_keys
```

### 2. 密钥格式不兼容
**问题**：新版本OpenSSH生成的密钥格式不被老版本支持
**解决方案**：使用PEM格式
```bash
ssh-keygen -t rsa -b 4096 -m PEM
```

### 3. SSH.NET 无法读取密钥
**问题**：`PrivateKeyFile` 抛出异常
**解决方案**：
- 确保密钥格式正确（支持OpenSSH、PEM格式）
- 检查密码短语是否正确
- 验证文件路径和权限

### 4. 服务器日志排查
查看SSH服务器日志：
```bash
sudo tail -f /var/log/auth.log    # Ubuntu/Debian
sudo tail -f /var/log/secure      # CentOS/RHEL
```

## 项目中的应用建议

### 1. 配置存储
在TOML配置中支持私钥路径：
```toml
[remote_device.auth]
type = "key"
private_key_path = "~/.ssh/id_rsa"
passphrase = ""  # 可选，或从安全存储读取
```

### 2. 认证流程优化
1. 优先尝试密钥认证
2. 失败时回退到密码认证
3. 提供UI引导用户配置密钥

### 3. 安全存储
- 密码短语不应明文存储
- 考虑使用Windows Credential Manager或类似安全存储

## 相关文档

- [SSH.NET 使用指南](SSH.NET-使用指南.md)
- [SSH.NET 文件同步指南](SSH.NET-File-Sync-Guide.md)
- [Terminal TOML 配置设计](Terminal-TOML-Configuration-Design.md)

---

**维护说明**：本文档由SSH连接专家维护，其他AI在实际开发中遇到相关问题时，请及时更新此文档。
