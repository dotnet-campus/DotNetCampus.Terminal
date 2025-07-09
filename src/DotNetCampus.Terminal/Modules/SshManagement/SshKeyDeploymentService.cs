using System.Security;
using DotNetCampus.Logging;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using Renci.SshNet;

namespace DotNetCampus.Terminal.Modules.SshManagement;

/// <summary>
/// SSH密钥部署服务 - 负责执行一键部署SSH密钥的核心逻辑
/// </summary>
public class SshKeyDeploymentService
{
    /// <summary>
    /// 为设备部署SSH密钥
    /// </summary>
    /// <param name="device">SSH设备信息</param>
    /// <param name="password">设备密码</param>
    /// <param name="passphrase">私钥密码短语（可选）</param>
    /// <param name="disablePasswordAuth">是否禁用密码认证</param>
    /// <param name="progress">进度回调</param>
    /// <returns>部署结果</returns>
    public async Task<KeyDeploymentResult> DeployKeyToDeviceAsync(
        SshRemoteDeviceInfo device,
        string password,
        string? passphrase = null,
        bool disablePasswordAuth = false,
        IProgress<(string step, int percent)>? progress = null)
    {
        var result = new KeyDeploymentResult();

        try
        {
            progress?.Report(("开始密钥部署流程...", 0));

            // 1. 检测或生成全局密钥
            result.AddStep("检测现有SSH密钥...");
            progress?.Report(("检测现有SSH密钥", 10));

            var privateKeyPath = GlobalSshKeyManager.FindExistingPrivateKey();

            if (privateKeyPath == null)
            {
                result.AddStep("未找到现有SSH密钥，将生成新的密钥对...");
                progress?.Report(("生成新的SSH密钥对", 20));

                privateKeyPath = await SshKeyGenerator.GenerateGlobalKeyPairAsync();
                result.AddSuccess($"成功生成新密钥: {Path.GetFileName(privateKeyPath)}");
            }
            else
            {
                var keyFormat = GlobalSshKeyManager.DetectKeyFormat(privateKeyPath);
                result.AddSuccess($"找到现有密钥: {Path.GetFileName(privateKeyPath)} ({keyFormat})");
            }

            result.PrivateKeyPath = privateKeyPath;

            // 2. 检查密钥是否需要密码短语
            progress?.Report(("验证密钥文件", 30));
            bool needsPassphrase = false;
            try
            {
                var testKey = new PrivateKeyFile(privateKeyPath);
                result.AddStep("密钥文件验证成功（无密码短语保护）");
            }
            catch (Exception ex) when (IsPassphraseRequired(ex))
            {
                needsPassphrase = true;
                result.AddWarning("检测到密钥需要密码短语保护");

                if (string.IsNullOrEmpty(passphrase))
                {
                    result.RequiresPassphrase = true;
                    result.AddError("密钥需要密码短语，但未提供密码短语");
                    return result; // 返回给UI处理密码短语输入
                }
            }

            // 3. 获取公钥内容
            progress?.Report(("读取公钥内容", 40));
            result.AddStep("正在读取公钥内容...");
            var publicKeyContent = GlobalSshKeyManager.GetPublicKeyContent(privateKeyPath);
            result.AddStep("成功读取公钥内容");

            // 4. 连接到远程设备
            progress?.Report(("连接到远程设备", 50));
            result.AddStep($"正在连接到远程设备 {device.UserName}@{device.Host}:{device.Port}...");

            var passwordAuth = new PasswordAuthenticationMethod(device.UserName, password);
            var connectionInfo = new ConnectionInfo(device.Host, device.Port, device.UserName, passwordAuth);

            using var client = new SshClient(connectionInfo);
            client.Connect();
            result.AddSuccess("已连接到远程设备");

            // 5. 部署公钥到远程设备
            progress?.Report(("部署公钥到远程设备", 70));
            await DeployPublicKeyAsync(client, publicKeyContent, result);

            // 6. 验证密钥认证
            progress?.Report(("验证密钥认证", 85));
            result.AddStep("正在验证密钥认证...");

            PrivateKeyFile keyFile;
            if (needsPassphrase && !string.IsNullOrEmpty(passphrase))
            {
                keyFile = new PrivateKeyFile(privateKeyPath, passphrase);
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
                result.AddSuccess("密钥认证验证成功！");

                // 7. 可选：禁用密码认证
                if (disablePasswordAuth)
                {
                    progress?.Report(("禁用密码认证", 95));
                    await DisablePasswordAuthenticationAsync(testClient, result);
                }

                result.Success = true;
                result.AddSuccess("SSH密钥部署完成！");
                progress?.Report(("部署完成", 100));
            }
            else
            {
                throw new SecurityException("密钥认证验证失败");
            }
        }
        catch (Exception ex)
        {
            result.AddError($"部署失败: {ex.Message}");
            Log.Error($"[SSH] 密钥部署失败: {ex.Message}", ex);
        }

        return result;
    }

    /// <summary>
    /// 部署公钥到远程设备
    /// </summary>
    /// <param name="client">SSH客户端</param>
    /// <param name="publicKeyContent">公钥内容</param>
    /// <param name="result">结果对象</param>
    private async Task DeployPublicKeyAsync(SshClient client, string publicKeyContent, KeyDeploymentResult result)
    {
        var commands = new[]
        {
            "mkdir -p ~/.ssh",
            "chmod 700 ~/.ssh",
            $"echo '{publicKeyContent}' >> ~/.ssh/authorized_keys",
            "chmod 600 ~/.ssh/authorized_keys",
            // 去重：移除重复的公钥条目
            "sort ~/.ssh/authorized_keys | uniq > ~/.ssh/authorized_keys.tmp && mv ~/.ssh/authorized_keys.tmp ~/.ssh/authorized_keys"
        };

        result.AddStep("正在部署公钥到远程设备...");

        foreach (var command in commands)
        {
            var commandResult = client.RunCommand(command);
            if (commandResult.ExitStatus != 0)
            {
                throw new InvalidOperationException($"命令执行失败: {command}, 错误: {commandResult.Error}");
            }
        }

        result.AddSuccess("公钥已成功部署到远程设备");

        // 验证authorized_keys文件
        var verifyCommand = client.RunCommand("test -f ~/.ssh/authorized_keys && echo 'EXISTS'");
        if (verifyCommand.Result.Trim() == "EXISTS")
        {
            result.AddStep("已验证authorized_keys文件存在");
        }
        else
        {
            result.AddWarning("无法验证authorized_keys文件");
        }
    }

    /// <summary>
    /// 禁用SSH密码认证（可选）
    /// </summary>
    /// <param name="client">SSH客户端</param>
    /// <param name="result">结果对象</param>
    private async Task DisablePasswordAuthenticationAsync(SshClient client, KeyDeploymentResult result)
    {
        try
        {
            result.AddStep("正在禁用SSH密码认证...");

            // 检查是否有sudo权限
            var sudoTest = client.RunCommand("sudo -n echo 'test' 2>/dev/null");
            bool hasSudo = sudoTest.ExitStatus == 0;

            if (!hasSudo)
            {
                result.AddWarning("没有sudo权限，无法禁用密码认证，需要手动配置");
                return;
            }

            // 备份sshd_config
            var backupCmd = client.RunCommand("sudo cp /etc/ssh/sshd_config /etc/ssh/sshd_config.backup");
            if (backupCmd.ExitStatus == 0)
            {
                result.AddStep("已备份SSH配置文件");
            }

            // 禁用密码认证
            var commands = new[]
            {
                "sudo sed -i 's/#*PasswordAuthentication.*/PasswordAuthentication no/' /etc/ssh/sshd_config",
                "sudo sed -i 's/#*ChallengeResponseAuthentication.*/ChallengeResponseAuthentication no/' /etc/ssh/sshd_config",
                "sudo sed -i 's/#*UsePAM.*/UsePAM no/' /etc/ssh/sshd_config"
            };

            foreach (var command in commands)
            {
                var cmdResult = client.RunCommand(command);
                if (cmdResult.ExitStatus != 0)
                {
                    result.AddWarning($"配置命令执行失败: {command}");
                }
            }

            // 验证配置更改
            var verifyCmd = client.RunCommand("sudo sshd -t");
            if (verifyCmd.ExitStatus == 0)
            {
                result.AddStep("SSH配置语法验证通过");

                // 重启SSH服务
                var restartCmd = client.RunCommand("sudo systemctl restart sshd || sudo service ssh restart");
                if (restartCmd.ExitStatus == 0)
                {
                    result.AddSuccess("SSH服务已重启，密码认证已禁用");
                }
                else
                {
                    result.AddWarning("SSH服务重启失败，配置可能需要手动重启生效");
                }
            }
            else
            {
                result.AddWarning("SSH配置验证失败，可能需要手动检查配置");
            }
        }
        catch (Exception ex)
        {
            result.AddWarning($"禁用密码认证时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 回滚SSH密钥部署（移除公钥）
    /// </summary>
    /// <param name="device">SSH设备信息</param>
    /// <param name="password">设备密码</param>
    /// <returns>回滚结果</returns>
    public async Task<KeyDeploymentResult> RollbackKeyDeploymentAsync(SshRemoteDeviceInfo device, string password)
    {
        var result = new KeyDeploymentResult();

        try
        {
            result.AddStep("开始回滚SSH密钥部署...");

            var privateKeyPath = GlobalSshKeyManager.FindExistingPrivateKey();
            if (privateKeyPath == null)
            {
                result.AddError("未找到本地SSH密钥，无法执行回滚");
                return result;
            }

            var publicKeyContent = GlobalSshKeyManager.GetPublicKeyContent(privateKeyPath);

            // 连接到远程设备
            var passwordAuth = new PasswordAuthenticationMethod(device.UserName, password);
            var connectionInfo = new ConnectionInfo(device.Host, device.Port, device.UserName, passwordAuth);

            using var client = new SshClient(connectionInfo);
            client.Connect();
            result.AddStep("已连接到远程设备");

            // 从authorized_keys中移除公钥
            var removeKeyCmd = $"grep -v '{publicKeyContent.Trim()}' ~/.ssh/authorized_keys > ~/.ssh/authorized_keys.tmp && mv ~/.ssh/authorized_keys.tmp ~/.ssh/authorized_keys";
            var removeResult = client.RunCommand(removeKeyCmd);

            if (removeResult.ExitStatus == 0)
            {
                result.AddSuccess("已从authorized_keys中移除公钥");
                result.Success = true;
            }
            else
            {
                result.AddError($"移除公钥失败: {removeResult.Error}");
            }
        }
        catch (Exception ex)
        {
            result.AddError($"回滚失败: {ex.Message}");
            Log.Error($"[SSH] 密钥回滚失败: {ex.Message}", ex);
        }

        return result;
    }

    /// <summary>
    /// 检测异常是否表示需要密码短语
    /// </summary>
    /// <param name="ex">异常对象</param>
    /// <returns>是否需要密码短语</returns>
    private static bool IsPassphraseRequired(Exception ex)
    {
        var message = ex.Message.ToLower();
        return message.Contains("passphrase") ||
               message.Contains("encrypted") ||
               message.Contains("password") ||
               message.Contains("decrypt");
    }
}
