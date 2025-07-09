using DotNetCampus.Logging;
using Renci.SshNet;

namespace DotNetCampus.Terminal.FileSync.Core;

/// <summary>
/// SFTP 操作辅助工具
/// </summary>
public class SftpOperationHelper
{
    /// <summary>
    /// 确保远程目录存在
    /// </summary>
    public void EnsureRemoteDirectoryExists(SftpClient client, string remoteDirectory)
    {
        string[] directories = remoteDirectory.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        string currentPath = "/";

        foreach (string directory in directories)
        {
            currentPath = Path.Combine(currentPath, directory).Replace('\\', '/');

            if (!client.Exists(currentPath))
            {
                Log.Debug($"[FileSync] 创建远程目录: {currentPath}");
                client.CreateDirectory(currentPath);
            }
        }
    }
}
