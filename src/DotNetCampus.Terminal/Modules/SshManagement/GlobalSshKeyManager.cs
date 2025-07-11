using System.Diagnostics;
using DotNetCampus.Logging;
using Renci.SshNet;

namespace DotNetCampus.Terminal.Modules.SshManagement;

/// <summary>
/// 全局SSH密钥管理器 - 负责检测、生成和管理全局SSH密钥对
/// </summary>
public static class GlobalSshKeyManager
{
    // 扩展密钥检测列表，覆盖更多常见的命名方式
    private static readonly string[] KeyPriority =
    [
        // 标准命名
        "id_ed25519",     // Ed25519 (现代、安全)
        "id_rsa",         // RSA (广泛兼容)
        "id_ecdsa",       // ECDSA (椭圆曲线)
        "id_dsa",         // DSA (已废弃，但可能存在)

        // 其他常见命名方式
        "ssh_host_rsa_key",     // 某些工具生成的命名
        "github_rsa",           // GitHub专用密钥
        "gitlab_rsa",           // GitLab专用密钥
    ];

    /// <summary>
    /// 查找现有的SSH私钥文件
    /// </summary>
    /// <returns>找到的私钥文件路径，如果没有找到则返回null</returns>
    public static string? FindExistingPrivateKey()
    {
        var sshDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");

        if (!Directory.Exists(sshDir))
        {
            Log.Info("[SSH] SSH目录不存在，未找到现有密钥");
            return null;
        }

        // 按优先级顺序查找密钥
        foreach (var keyName in KeyPriority)
        {
            var keyPath = Path.Combine(sshDir, keyName);
            if (IsValidPrivateKey(keyPath))
            {
                Log.Info($"[SSH] 找到有效的私钥文件: {keyPath}");
                return keyPath;
            }
        }

        // 如果按名称没找到，搜索目录中的所有文件
        try
        {
            var allFiles = Directory.GetFiles(sshDir)
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
        }
        catch (Exception ex)
        {
            Log.Warn($"[SSH] 搜索SSH目录时出错: {ex.Message}");
        }

        Log.Info("[SSH] 未发现任何SSH私钥");
        return null;
    }

    /// <summary>
    /// 验证私钥文件是否有效
    /// </summary>
    /// <param name="keyPath">私钥文件路径</param>
    /// <returns>是否是有效的私钥文件</returns>
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
                "-----BEGIN ENCRYPTED PRIVATE KEY-----", // 加密的私钥
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
                    Log.Warn($"[SSH] 私钥文件可能需要密码短语: {keyPath}");
                    return true; // 仍然认为是有效的，但需要用户输入密码短语
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Log.Warn($"[SSH] 验证私钥文件时出错: {keyPath}, 错误: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 获取私钥对应的公钥内容
    /// </summary>
    /// <param name="privateKeyPath">私钥文件路径</param>
    /// <returns>公钥内容</returns>
    public static string GetPublicKeyContent(string privateKeyPath)
    {
        // 尝试多种公钥文件命名方式
        var possiblePublicKeyPaths = new[]
        {
            privateKeyPath + ".pub",                    // 标准命名
            privateKeyPath.Replace("_key", "_key.pub"), // 某些工具的命名习惯
            Path.ChangeExtension(privateKeyPath, ".pub"), // 直接替换扩展名
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

    /// <summary>
    /// 从私钥生成公钥内容
    /// </summary>
    /// <param name="privateKeyPath">私钥文件路径</param>
    /// <returns>公钥内容</returns>
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
                RedirectStandardError = true,
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("无法启动ssh-keygen进程");
            }

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

    /// <summary>
    /// 检测密钥格式
    /// </summary>
    /// <param name="keyPath">密钥文件路径</param>
    /// <returns>密钥格式描述</returns>
    public static string DetectKeyFormat(string keyPath)
    {
        try
        {
            var firstLine = File.ReadLines(keyPath).FirstOrDefault()?.Trim();

            return firstLine switch
            {
                var line when line?.StartsWith("-----BEGIN OPENSSH PRIVATE KEY-----") == true => "OpenSSH",
                var line when line?.StartsWith("-----BEGIN RSA PRIVATE KEY-----") == true => "RSA (PEM)",
                var line when line?.StartsWith("-----BEGIN EC PRIVATE KEY-----") == true => "ECDSA (PEM)",
                var line when line?.StartsWith("-----BEGIN DSA PRIVATE KEY-----") == true => "DSA (PEM)",
                var line when line?.StartsWith("-----BEGIN PRIVATE KEY-----") == true => "PKCS#8",
                var line when line?.StartsWith("-----BEGIN ENCRYPTED PRIVATE KEY-----") == true => "Encrypted PKCS#8",
                _ => "Unknown",
            };
        }
        catch
        {
            return "Error";
        }
    }

    /// <summary>
    /// 检测密钥是否受SSH.NET支持
    /// </summary>
    /// <param name="keyPath">密钥文件路径</param>
    /// <returns>是否支持</returns>
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
