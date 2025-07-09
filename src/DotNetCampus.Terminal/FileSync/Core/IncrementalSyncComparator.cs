using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync.Models;

namespace DotNetCampus.Terminal.FileSync.Core;

/// <summary>
/// 增量同步比较器
/// </summary>
public class IncrementalSyncComparator
{
    /// <summary>
    /// 获取需要从本地同步到远程的文件列表
    /// </summary>
    public List<string> GetFilesToSyncLocalToRemote(
        Dictionary<string, FileChangeInfo> localFiles, 
        Dictionary<string, FileChangeInfo> remoteFiles,
        string localBasePath,
        string remoteBasePath)
    {
        var filesToSync = new List<string>();

        foreach (var (relativePath, localFile) in localFiles)
        {
            bool needSync = false;

            if (!remoteFiles.TryGetValue(relativePath, out var remoteFile))
            {
                // 远程文件不存在，需要同步
                needSync = true;
                Log.Debug($"[FileSync] 新文件需要上传: {relativePath}");
            }
            else
            {
                // 比较文件大小和修改时间
                if (localFile.Size != remoteFile.Size || localFile.LastWriteTime > remoteFile.LastWriteTime)
                {
                    needSync = true;
                    Log.Debug($"[FileSync] 文件已修改需要上传: {relativePath} (本地: {localFile.LastWriteTime}, 远程: {remoteFile.LastWriteTime})");
                }
            }

            if (needSync)
            {
                filesToSync.Add(localFile.FilePath);
            }
        }

        return filesToSync;
    }

    /// <summary>
    /// 获取需要从远程同步到本地的文件列表
    /// </summary>
    public List<string> GetFilesToSyncRemoteToLocal(
        Dictionary<string, FileChangeInfo> remoteFiles, 
        Dictionary<string, FileChangeInfo> localFiles,
        string remoteBasePath,
        string localBasePath)
    {
        var filesToSync = new List<string>();

        foreach (var (relativePath, remoteFile) in remoteFiles)
        {
            bool needSync = false;

            if (!localFiles.TryGetValue(relativePath, out var localFile))
            {
                // 本地文件不存在，需要同步
                needSync = true;
                Log.Debug($"[FileSync] 新文件需要下载: {relativePath}");
            }
            else
            {
                // 比较文件大小和修改时间
                if (remoteFile.Size != localFile.Size || remoteFile.LastWriteTime > localFile.LastWriteTime)
                {
                    needSync = true;
                    Log.Debug($"[FileSync] 文件已修改需要下载: {relativePath} (远程: {remoteFile.LastWriteTime}, 本地: {localFile.LastWriteTime})");
                }
            }

            if (needSync)
            {
                filesToSync.Add(remoteFile.FilePath);
            }
        }

        return filesToSync;
    }
}
