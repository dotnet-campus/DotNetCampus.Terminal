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
        var directories = remoteDirectory.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
        var currentPath = "/";

        foreach (var directory in directories)
        {
            currentPath = Path.Combine(currentPath, directory).Replace('\\', '/');

            if (!client.Exists(currentPath))
            {
                Log.Debug($"[FileSync] 创建远程目录: {currentPath}");
                client.CreateDirectory(currentPath);
            }
        }
    }

    /// <summary>
    /// 同步远程文件的时间戳到本地文件的时间戳
    /// </summary>
    /// <param name="client">SFTP客户端</param>
    /// <param name="remoteFilePath">远程文件路径</param>
    /// <param name="localFilePath">本地文件路径</param>
    public void SyncRemoteFileTimestamps(SftpClient client, string remoteFilePath, string localFilePath)
    {
        try
        {
            // 获取本地文件的时间戳信息
            var localFileInfo = new FileInfo(localFilePath);
            if (!localFileInfo.Exists)
            {
                Log.Warn($"[FileSync] 本地文件不存在，无法同步时间戳: {localFilePath}");
                return;
            }

            // 记录同步前的远程文件时间戳
            var beforeSync = client.GetAttributes(remoteFilePath);
            Log.Debug($"[FileSync] 同步前远程文件时间戳: {remoteFilePath} (修改时间: {beforeSync.LastWriteTime})");
            Log.Debug($"[FileSync] 本地文件时间戳: {localFilePath} (修改时间: {localFileInfo.LastWriteTime})");

            // 方法1：使用 SetAttributes 方法
            try
            {
                var remoteAttributes = client.GetAttributes(remoteFilePath);
                remoteAttributes.LastWriteTime = localFileInfo.LastWriteTime;
                remoteAttributes.LastAccessTime = localFileInfo.LastWriteTime;
                client.SetAttributes(remoteFilePath, remoteAttributes);
                
                // 验证时间戳是否设置成功
                VerifyRemoteFileTimestamp(client, remoteFilePath, localFileInfo.LastWriteTime);
                Log.Debug($"[FileSync] 已同步文件时间戳到远程(SetAttributes): {remoteFilePath} (修改时间: {localFileInfo.LastWriteTime})");
                return;
            }
            catch (Exception ex1)
            {
                Log.Debug($"[FileSync] SetAttributes 方法失败，尝试备用方法: {ex1.Message}");
            }

            // 方法2：使用 SetLastWriteTime 方法（备用）
            try
            {
                client.SetLastWriteTime(remoteFilePath, localFileInfo.LastWriteTime);
                
                // 验证时间戳是否设置成功
                VerifyRemoteFileTimestamp(client, remoteFilePath, localFileInfo.LastWriteTime);
                Log.Debug($"[FileSync] 已同步文件时间戳到远程(SetLastWriteTime): {remoteFilePath} (修改时间: {localFileInfo.LastWriteTime})");
                return;
            }
            catch (Exception ex2)
            {
                Log.Debug($"[FileSync] SetLastWriteTime 方法也失败: {ex2.Message}");
            }

            Log.Warn($"[FileSync] 所有时间戳同步方法都失败，可能服务器不支持此操作: {remoteFilePath}");
        }
        catch (Exception ex)
        {
            Log.Warn($"[FileSync] 同步远程文件时间戳失败: {remoteFilePath}. 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 同步本地文件的时间戳到远程文件的时间戳
    /// </summary>
    /// <param name="client">SFTP客户端</param>
    /// <param name="remoteFilePath">远程文件路径</param>
    /// <param name="localFilePath">本地文件路径</param>
    public void SyncLocalFileTimestamps(SftpClient client, string remoteFilePath, string localFilePath)
    {
        try
        {
            // 获取远程文件的时间戳信息
            var remoteFileAttributes = client.GetAttributes(remoteFilePath);

            // 设置本地文件的创建时间和修改时间
            var localFileInfo = new FileInfo(localFilePath);
            if (localFileInfo.Exists)
            {
                // 记录同步前的本地文件时间戳
                Log.Debug($"[FileSync] 同步前本地文件时间戳: {localFilePath} (修改时间: {localFileInfo.LastWriteTime})");
                Log.Debug($"[FileSync] 远程文件时间戳: {remoteFilePath} (修改时间: {remoteFileAttributes.LastWriteTime})");

                localFileInfo.CreationTime = remoteFileAttributes.LastWriteTime;
                localFileInfo.LastWriteTime = remoteFileAttributes.LastWriteTime;

                Log.Debug($"[FileSync] 已同步文件时间戳到本地: {localFilePath} (修改时间: {remoteFileAttributes.LastWriteTime})");
            }
            else
            {
                Log.Warn($"[FileSync] 本地文件不存在，无法同步时间戳: {localFilePath}");
            }
        }
        catch (Exception ex)
        {
            Log.Warn($"[FileSync] 同步本地文件时间戳失败: {localFilePath}. 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证远程文件时间戳是否同步成功
    /// </summary>
    /// <param name="client">SFTP客户端</param>
    /// <param name="remoteFilePath">远程文件路径</param>
    /// <param name="expectedTime">期望的时间戳</param>
    private void VerifyRemoteFileTimestamp(SftpClient client, string remoteFilePath, DateTime expectedTime)
    {
        try
        {
            var remoteAttributes = client.GetAttributes(remoteFilePath);
            var timeDifference = Math.Abs((remoteAttributes.LastWriteTime - expectedTime).TotalSeconds);
            
            if (timeDifference <= 2) // 允许2秒的误差
            {
                Log.Debug($"[FileSync] 远程文件时间戳验证成功: {remoteFilePath} (期望: {expectedTime}, 实际: {remoteAttributes.LastWriteTime}, 差异: {timeDifference}秒)");
            }
            else
            {
                Log.Warn($"[FileSync] 远程文件时间戳验证失败: {remoteFilePath} (期望: {expectedTime}, 实际: {remoteAttributes.LastWriteTime}, 差异: {timeDifference}秒)");
            }
        }
        catch (Exception ex)
        {
            Log.Warn($"[FileSync] 无法验证远程文件时间戳: {remoteFilePath}. 错误: {ex.Message}");
        }
    }
}
