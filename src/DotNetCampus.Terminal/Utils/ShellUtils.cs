using DotNetCampus.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotNetCampus.Terminal.Utils;

/// <summary>
/// Shell管理工具类，用于在新的终端标签页或窗口中打开SSH连接
/// </summary>
public static class ShellUtils
{
    /// <summary>
    /// 在新的终端标签页中打开SSH连接
    /// </summary>
    /// <param name="host">主机地址</param>
    /// <param name="port">端口号</param>
    /// <param name="userName">用户名</param>
    /// <param name="password">密码（可选）</param>
    /// <returns>是否成功启动</returns>
    public static async Task<bool> OpenSshInNewTabAsync(string host, int port, string userName, string? password = null)
    {
        try
        {
            Log.Info($"[Shell] 尝试在新标签页打开SSH连接到 {userName}@{host}:{port}");

            var terminalType = DetectTerminalType();
            var sshCommand = BuildSshCommand(host, port, userName, password);

            return await LaunchInNewTabAsync(terminalType, sshCommand);
        }
        catch (Exception ex)
        {
            Log.Error($"[Shell] 打开SSH连接失败: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// 检测当前运行的终端模拟器类型
    /// </summary>
    private static TerminalType DetectTerminalType()
    {
        // 通过环境变量检测终端类型
        var term = Environment.GetEnvironmentVariable("TERM");
        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        var sessionName = Environment.GetEnvironmentVariable("SESSION_NAME");
        var wslDistroName = Environment.GetEnvironmentVariable("WSL_DISTRO_NAME");

        Log.Info($"[Shell] 检测终端环境: TERM={term}, TERM_PROGRAM={termProgram}, SESSION_NAME={sessionName}");

        // Windows Terminal
        if (termProgram?.Contains("vscode") == true)
        {
            return TerminalType.VSCode;
        }
        if (!string.IsNullOrEmpty(sessionName) || termProgram?.Contains("WindowsTerminal") == true)
        {
            return TerminalType.WindowsTerminal;
        }

        // PowerShell
        if (termProgram?.Contains("PowerShell") == true)
        {
            return TerminalType.PowerShell;
        }

        // WSL
        if (!string.IsNullOrEmpty(wslDistroName))
        {
            return TerminalType.WSL;
        }

        // Hyper, iTerm2, Terminal (macOS)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (termProgram?.Contains("iTerm") == true)
                return TerminalType.ITerm2;
            if (termProgram?.Contains("Terminal") == true)
                return TerminalType.MacTerminal;
        }

        // Linux terminals
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (termProgram?.Contains("gnome-terminal") == true)
                return TerminalType.GnomeTerminal;
            if (term?.Contains("xterm") == true)
                return TerminalType.XTerm;
        }

        // 默认尝试Windows Terminal
        return TerminalType.WindowsTerminal;
    }

    /// <summary>
    /// 构建SSH连接命令
    /// </summary>
    private static string BuildSshCommand(string host, int port, string userName, string? password)
    {
        var sshCmd = $"ssh {userName}@{host}";

        if (port != 22)
        {
            sshCmd += $" -p {port}";
        }

        // 如果有密码，可以使用sshpass（Linux/macOS）或其他方式
        // 但为了安全起见，通常建议让用户手动输入密码
        Log.Info($"[Shell] 构建SSH命令: {sshCmd}");

        return sshCmd;
    }

    /// <summary>
    /// 在新标签页中启动命令
    /// </summary>
    private static async Task<bool> LaunchInNewTabAsync(TerminalType terminalType, string command)
    {
        return terminalType switch
        {
            TerminalType.WindowsTerminal => await LaunchWindowsTerminalTabAsync(command),
            TerminalType.VSCode => await LaunchVSCodeTerminalAsync(command),
            TerminalType.PowerShell => await LaunchPowerShellTabAsync(command),
            TerminalType.ITerm2 => await LaunchITerm2TabAsync(command),
            TerminalType.MacTerminal => await LaunchMacTerminalTabAsync(command),
            TerminalType.GnomeTerminal => await LaunchGnomeTerminalTabAsync(command),
            TerminalType.XTerm => await LaunchXTermAsync(command),
            _ => await LaunchFallbackAsync(command),
        };
    }

    /// <summary>
    /// Windows Terminal新标签页
    /// </summary>
    private static async Task<bool> LaunchWindowsTerminalTabAsync(string command)
    {
        try
        {
            // Windows Terminal支持通过wt命令打开新标签页
            var startInfo = new ProcessStartInfo
            {
                FileName = "wt",
                Arguments = $"new-tab {command}",
                UseShellExecute = false,
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Shell] Windows Terminal启动失败: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// VS Code终端新标签页
    /// </summary>
    private static async Task<bool> LaunchVSCodeTerminalAsync(string command)
    {
        try
        {
            // VS Code可以通过code命令打开新终端
            var startInfo = new ProcessStartInfo
            {
                FileName = "code",
                Arguments = $"--new-window --terminal-cmd \"{command}\"",
                UseShellExecute = false,
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Shell] VS Code终端启动失败: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// PowerShell新窗口
    /// </summary>
    private static async Task<bool> LaunchPowerShellTabAsync(string command)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoExit -Command \"{command}\"",
                UseShellExecute = true,
            };

            using var process = Process.Start(startInfo);
            return process != null;
        }
        catch (Exception ex)
        {
            Log.Error($"[Shell] PowerShell启动失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// iTerm2新标签页（macOS）
    /// </summary>
    private static async Task<bool> LaunchITerm2TabAsync(string command)
    {
        try
        {
            var appleScript = $@"
                tell application ""iTerm""
                    tell current window
                        create tab with default profile
                        tell current session
                            write text ""{command}""
                        end tell
                    end tell
                end tell";

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e '{appleScript}'",
                UseShellExecute = false,
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Shell] iTerm2启动失败: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// macOS Terminal新标签页
    /// </summary>
    private static async Task<bool> LaunchMacTerminalTabAsync(string command)
    {
        try
        {
            var appleScript = $@"
                tell application ""Terminal""
                    activate
                    tell application ""System Events""
                        keystroke ""t"" using {{command down}}
                    end tell
                    do script ""{command}"" in front window
                end tell";

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e '{appleScript}'",
                UseShellExecute = false,
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Shell] macOS Terminal启动失败: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// GNOME Terminal新标签页
    /// </summary>
    private static async Task<bool> LaunchGnomeTerminalTabAsync(string command)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "gnome-terminal",
                Arguments = $"--tab -e \"{command}\"",
                UseShellExecute = false,
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Shell] GNOME Terminal启动失败: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// XTerm新窗口
    /// </summary>
    private static async Task<bool> LaunchXTermAsync(string command)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "xterm",
                Arguments = $"-e {command}",
                UseShellExecute = false,
            };

            using var process = Process.Start(startInfo);
            return process != null;
        }
        catch (Exception ex)
        {
            Log.Error($"[Shell] XTerm启动失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 回退方案：尝试在新的cmd/bash窗口中启动
    /// </summary>
    private static async Task<bool> LaunchFallbackAsync(string command)
    {
        try
        {
            Log.Info("[Shell] 使用回退方案启动新终端窗口");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows回退：使用cmd新窗口
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/k {command}",
                    UseShellExecute = true,
                };

                using var process = Process.Start(startInfo);
                return process != null;
            }
            else
            {
                // Linux/macOS回退：使用bash新窗口
                var startInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{command}; exec bash\"",
                    UseShellExecute = true,
                };

                using var process = Process.Start(startInfo);
                return process != null;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Shell] 回退方案启动失败: {ex.Message}");
            return false;
        }
    }
}

/// <summary>
/// 终端类型枚举
/// </summary>
public enum TerminalType
{
    /// <summary>
    /// Windows Terminal
    /// </summary>
    WindowsTerminal,

    /// <summary>
    /// VS Code集成终端
    /// </summary>
    VSCode,

    /// <summary>
    /// PowerShell
    /// </summary>
    PowerShell,

    /// <summary>
    /// WSL终端
    /// </summary>
    WSL,

    /// <summary>
    /// iTerm2 (macOS)
    /// </summary>
    ITerm2,

    /// <summary>
    /// macOS Terminal
    /// </summary>
    MacTerminal,

    /// <summary>
    /// GNOME Terminal
    /// </summary>
    GnomeTerminal,

    /// <summary>
    /// XTerm
    /// </summary>
    XTerm,
}
