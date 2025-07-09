using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync.Models;
using Renci.SshNet;

namespace DotNetCampus.Terminal.FileSync.Core;

/// <summary>
/// 远程文件信息获取器
/// </summary>
public class RemoteFileInfoProvider
{
    /// <summary>
    /// 获取远程文件信息用于增量同步
    /// </summary>
    public Dictionary<string, FileChangeInfo> GetFileInfos(SftpClient client, string remotePath)
    {
        var fileInfos = new Dictionary<string, FileChangeInfo>();

        try
        {
            GetFileInfosRecursive(client, remotePath, remotePath, fileInfos);
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 获取远程文件信息时发生错误: {remotePath}. 错误: {ex.Message}");
        }

        return fileInfos;
    }

    /// <summary>
    /// 递归获取远程文件信息
    /// </summary>
    private void GetFileInfosRecursive(SftpClient client, string currentPath, string basePath, Dictionary<string, FileChangeInfo> fileInfos)
    {
        try
        {
            var entries = client.ListDirectory(currentPath);

            foreach (var entry in entries)
            {
                // 跳过 . 和 .. 目录
                if (entry.Name == "." || entry.Name == "..")
                    continue;

                var fullPath = $"{currentPath.TrimEnd('/')}/{entry.Name}";

                if (entry.IsDirectory)
                {
                    // 递归处理子目录
                    GetFileInfosRecursive(client, fullPath, basePath, fileInfos);
                }
                else if (entry.IsRegularFile)
                {
                    var relativePath = fullPath.Substring(basePath.Length).TrimStart('/');
                    
                    fileInfos[relativePath] = new FileChangeInfo
                    {
                        FilePath = fullPath,
                        Size = entry.Length,
                        LastWriteTime = entry.LastWriteTime
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 处理远程目录时发生错误: {currentPath}. 错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 递归获取远程目录中的所有文件
    /// </summary>
    public List<string> GetFiles(SftpClient client, string remotePath)
    {
        var files = new List<string>();

        try
        {
            GetFilesRecursive(client, remotePath, files);
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 获取远程文件列表时发生错误: {remotePath}. 错误: {ex.Message}");
        }

        return files;
    }

    /// <summary>
    /// 递归获取远程文件
    /// </summary>
    private void GetFilesRecursive(SftpClient client, string currentPath, List<string> files)
    {
        try
        {
            var entries = client.ListDirectory(currentPath);

            foreach (var entry in entries)
            {
                // 跳过 . 和 .. 目录
                if (entry.Name == "." || entry.Name == "..")
                    continue;

                var fullPath = $"{currentPath.TrimEnd('/')}/{entry.Name}";

                if (entry.IsDirectory)
                {
                    // 递归处理子目录
                    GetFilesRecursive(client, fullPath, files);
                }
                else if (entry.IsRegularFile)
                {
                    // 添加文件到列表
                    files.Add(fullPath);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 处理远程目录时发生错误: {currentPath}. 错误: {ex.Message}");
        }
    }
}
