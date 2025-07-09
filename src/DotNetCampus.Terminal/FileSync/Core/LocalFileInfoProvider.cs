using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync.Models;

namespace DotNetCampus.Terminal.FileSync.Core;

/// <summary>
/// 本地文件信息获取器
/// </summary>
public class LocalFileInfoProvider
{
    /// <summary>
    /// 获取本地文件信息用于增量同步
    /// </summary>
    public Dictionary<string, FileChangeInfo> GetFileInfos(string localPath)
    {
        var fileInfos = new Dictionary<string, FileChangeInfo>();

        try
        {
            var files = Directory.GetFiles(localPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var info = new System.IO.FileInfo(file);
                var relativePath = file.Substring(localPath.Length).TrimStart('\\', '/').Replace('\\', '/');
                
                fileInfos[relativePath] = new FileChangeInfo
                {
                    FilePath = file,
                    Size = info.Length,
                    LastWriteTime = info.LastWriteTime
                };
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 获取本地文件信息时发生错误: {localPath}. 错误: {ex.Message}");
        }

        return fileInfos;
    }

    /// <summary>
    /// 获取本地文件列表（兼容旧版本）
    /// </summary>
    public List<string> GetFiles(string localPath)
    {
        var files = new List<string>();

        try
        {
            files.AddRange(Directory.GetFiles(localPath, "*", SearchOption.AllDirectories));
        }
        catch (Exception ex)
        {
            Log.Error($"[FileSync] 获取本地文件列表时发生错误: {localPath}. 错误: {ex.Message}");
        }

        return files;
    }
}
