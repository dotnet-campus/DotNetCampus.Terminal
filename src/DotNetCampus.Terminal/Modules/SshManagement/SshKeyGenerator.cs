using System.Diagnostics;
using DotNetCampus.Logging;

namespace DotNetCampus.Terminal.Modules.SshManagement;

/// <summary>
/// SSH密钥生成器 - 负责生成新的SSH密钥对
/// </summary>
public static class SshKeyGenerator
{
    /// <summary>
    /// 生成全局SSH密钥对
    /// </summary>
    /// <param name="keyType">密钥类型，默认为ed25519</param>
    /// <param name="keySize">密钥大小（仅对RSA有效），默认为4096</param>
    /// <returns>生成的私钥文件路径</returns>
    public static async Task<string> GenerateGlobalKeyPairAsync(string keyType = "ed25519", int keySize = 4096)
    {
        var sshDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
        Directory.CreateDirectory(sshDir);

        var keyPath = keyType.ToLower() switch
        {
            "ed25519" => Path.Combine(sshDir, "id_ed25519"),
            "rsa" => Path.Combine(sshDir, "id_rsa"),
            "ecdsa" => Path.Combine(sshDir, "id_ecdsa"),
            _ => throw new ArgumentException($"不支持的密钥类型: {keyType}")
        };

        if (File.Exists(keyPath))
        {
            Log.Info($"[SSH] 全局密钥已存在: {keyPath}");
            return keyPath;
        }

        var comment = $"{Environment.UserName}@{Environment.MachineName}-{DateTime.Now:yyyyMMdd}";

        // 构建ssh-keygen命令参数
        var arguments = keyType.ToLower() switch
        {
            "ed25519" => $"-t ed25519 -f \"{keyPath}\" -C \"{comment}\" -N \"\"",
            "rsa" => $"-t rsa -b {keySize} -f \"{keyPath}\" -C \"{comment}\" -N \"\"",
            "ecdsa" => $"-t ecdsa -b 256 -f \"{keyPath}\" -C \"{comment}\" -N \"\"",
            _ => throw new ArgumentException($"不支持的密钥类型: {keyType}")
        };

        // 使用 ssh-keygen 生成密钥
        var startInfo = new ProcessStartInfo
        {
            FileName = "ssh-keygen",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Log.Info($"[SSH] 开始生成{keyType.ToUpper()}密钥对: {keyPath}");

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("无法启动ssh-keygen进程");
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"密钥生成失败: {error}");
        }

        // 验证密钥文件是否成功创建
        if (!File.Exists(keyPath))
        {
            throw new InvalidOperationException($"密钥文件生成失败: {keyPath}");
        }

        if (!File.Exists(keyPath + ".pub"))
        {
            throw new InvalidOperationException($"公钥文件生成失败: {keyPath}.pub");
        }

        // 设置正确的文件权限（在Unix系统上）
        if (!OperatingSystem.IsWindows())
        {
            try
            {
                SetFilePermissions(keyPath, "600");
                SetFilePermissions(keyPath + ".pub", "644");
                SetFilePermissions(sshDir, "700");
                Log.Info("[SSH] 已设置密钥文件权限");
            }
            catch (Exception ex)
            {
                Log.Warn($"[SSH] 设置文件权限时出错: {ex.Message}");
            }
        }

        Log.Info($"[SSH] 成功生成全局密钥对: {keyPath}");
        return keyPath;
    }

    /// <summary>
    /// 生成带密码短语的SSH密钥对
    /// </summary>
    /// <param name="passphrase">密码短语</param>
    /// <param name="keyType">密钥类型，默认为ed25519</param>
    /// <param name="keySize">密钥大小（仅对RSA有效），默认为4096</param>
    /// <returns>生成的私钥文件路径</returns>
    public static async Task<string> GenerateKeyPairWithPassphraseAsync(string passphrase, string keyType = "ed25519", int keySize = 4096)
    {
        if (string.IsNullOrEmpty(passphrase))
        {
            throw new ArgumentException("密码短语不能为空", nameof(passphrase));
        }

        var sshDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
        Directory.CreateDirectory(sshDir);

        var keyPath = keyType.ToLower() switch
        {
            "ed25519" => Path.Combine(sshDir, "id_ed25519"),
            "rsa" => Path.Combine(sshDir, "id_rsa"),
            "ecdsa" => Path.Combine(sshDir, "id_ecdsa"),
            _ => throw new ArgumentException($"不支持的密钥类型: {keyType}")
        };

        if (File.Exists(keyPath))
        {
            Log.Info($"[SSH] 密钥已存在，将覆盖: {keyPath}");
        }

        var comment = $"{Environment.UserName}@{Environment.MachineName}-{DateTime.Now:yyyyMMdd}";

        // 构建ssh-keygen命令参数（使用密码短语）
        var arguments = keyType.ToLower() switch
        {
            "ed25519" => $"-t ed25519 -f \"{keyPath}\" -C \"{comment}\" -N \"{passphrase}\"",
            "rsa" => $"-t rsa -b {keySize} -f \"{keyPath}\" -C \"{comment}\" -N \"{passphrase}\"",
            "ecdsa" => $"-t ecdsa -b 256 -f \"{keyPath}\" -C \"{comment}\" -N \"{passphrase}\"",
            _ => throw new ArgumentException($"不支持的密钥类型: {keyType}")
        };

        var startInfo = new ProcessStartInfo
        {
            FileName = "ssh-keygen",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Log.Info($"[SSH] 开始生成带密码短语的{keyType.ToUpper()}密钥对: {keyPath}");

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("无法启动ssh-keygen进程");
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"密钥生成失败: {error}");
        }

        // 验证密钥文件是否成功创建
        if (!File.Exists(keyPath) || !File.Exists(keyPath + ".pub"))
        {
            throw new InvalidOperationException("密钥文件生成失败");
        }

        // 设置正确的文件权限（在Unix系统上）
        if (!OperatingSystem.IsWindows())
        {
            try
            {
                SetFilePermissions(keyPath, "600");
                SetFilePermissions(keyPath + ".pub", "644");
                SetFilePermissions(sshDir, "700");
            }
            catch (Exception ex)
            {
                Log.Warn($"[SSH] 设置文件权限时出错: {ex.Message}");
            }
        }

        Log.Info($"[SSH] 成功生成带密码短语的全局密钥对: {keyPath}");
        return keyPath;
    }

    /// <summary>
    /// 检查ssh-keygen是否可用
    /// </summary>
    /// <returns>是否可用</returns>
    public static async Task<bool> IsSshKeygenAvailableAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ssh-keygen",
                Arguments = "--help",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return false;
            }

            await process.WaitForExitAsync();
            return true; // 不管退出码是什么，只要能运行就算可用
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 设置文件权限（Unix系统）
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="permissions">权限字符串（如"600"）</param>
    private static void SetFilePermissions(string filePath, string permissions)
    {
        if (OperatingSystem.IsWindows())
        {
            return; // Windows系统不需要设置Unix权限
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"{permissions} \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();
        }
        catch (Exception ex)
        {
            Log.Warn($"[SSH] 设置文件权限失败: {filePath}, 错误: {ex.Message}");
        }
    }
}
