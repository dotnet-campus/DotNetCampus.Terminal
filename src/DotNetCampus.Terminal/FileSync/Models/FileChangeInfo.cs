using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.FileSync.Models;

/// <summary>
/// 文件信息用于增量同步比较
/// </summary>
public record FileChangeInfo
{
    public required string FilePath { get; init; }
    public long Size { get; init; }
    public DateTimeOffset LastWriteTime { get; init; }
}
