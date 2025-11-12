using System.Text.Json.Serialization;

namespace DotNetCampus.Terminal.Modules.Configurations.Models;

/// <summary>
/// 同步方向
/// </summary>
public enum SyncDirection
{
    /// <summary>
    /// 本地到远程 (Push/Upload)
    /// </summary>
    LocalToRemote = 0,

    /// <summary>
    /// 远程到本地 (Pull/Download)
    /// </summary>
    RemoteToLocal = 1,
}

/// <summary>
/// 同步方向解析器，支持多种别名
/// </summary>
public static class SyncDirectionParser
{
    /// <summary>
    /// 将字符串解析为同步方向，支持多种别名
    /// </summary>
    /// <param name="direction">方向字符串</param>
    /// <returns>解析的同步方向</returns>
    public static SyncDirection Parse(string direction)
    {
        if (string.IsNullOrWhiteSpace(direction))
            return SyncDirection.LocalToRemote;

        var normalizedDirection = direction.Trim().ToLowerInvariant();

        return normalizedDirection switch
        {
            // 本地到远程的别名
            "localtoremote" or "local-to-remote" or "local_to_remote" => SyncDirection.LocalToRemote,
            "push" => SyncDirection.LocalToRemote,
            "upload" => SyncDirection.LocalToRemote,
            "up" => SyncDirection.LocalToRemote,
            "send" => SyncDirection.LocalToRemote,
            "出" or "上传" or "推送" => SyncDirection.LocalToRemote,

            // 远程到本地的别名
            "remotetolocal" or "remote-to-local" or "remote_to_local" => SyncDirection.RemoteToLocal,
            "pull" => SyncDirection.RemoteToLocal,
            "download" => SyncDirection.RemoteToLocal,
            "down" => SyncDirection.RemoteToLocal,
            "fetch" => SyncDirection.RemoteToLocal,
            "receive" => SyncDirection.RemoteToLocal,
            "入" or "下载" or "拉取" => SyncDirection.RemoteToLocal,

            // 默认值
            _ => SyncDirection.LocalToRemote,
        };
    }

    /// <summary>
    /// 将同步方向转换为友好的显示文本
    /// </summary>
    /// <param name="direction">同步方向</param>
    /// <returns>显示文本</returns>
    public static string ToDisplayText(SyncDirection direction)
    {
        return direction switch
        {
            SyncDirection.LocalToRemote => "本地 → 远程 (Push/Upload)",
            SyncDirection.RemoteToLocal => "远程 → 本地 (Pull/Download)",
            _ => "未知方向",
        };
    }

    /// <summary>
    /// 获取所有支持的别名
    /// </summary>
    /// <returns>支持的别名列表</returns>
    public static Dictionary<string, SyncDirection> GetSupportedAliases()
    {
        return new Dictionary<string, SyncDirection>
        {
            // 本地到远程
            ["LocalToRemote"] = SyncDirection.LocalToRemote,
            ["Push"] = SyncDirection.LocalToRemote,
            ["Upload"] = SyncDirection.LocalToRemote,
            ["Send"] = SyncDirection.LocalToRemote,
            ["上传"] = SyncDirection.LocalToRemote,
            ["推送"] = SyncDirection.LocalToRemote,

            // 远程到本地
            ["RemoteToLocal"] = SyncDirection.RemoteToLocal,
            ["Pull"] = SyncDirection.RemoteToLocal,
            ["Download"] = SyncDirection.RemoteToLocal,
            ["Fetch"] = SyncDirection.RemoteToLocal,
            ["下载"] = SyncDirection.RemoteToLocal,
            ["拉取"] = SyncDirection.RemoteToLocal,
        };
    }
}

/// <summary>
/// 同步目录配置
/// </summary>
public class DirectorySyncingModel
{
    /// <summary>
    /// 同步目录名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 远程路径
    /// </summary>
    public string RemotePath { get; set; } = string.Empty;

    /// <summary>
    /// 本地路径
    /// </summary>
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    private SyncDirection _direction = SyncDirection.LocalToRemote;

    /// <summary>
    /// 同步方向，默认为本地到远程
    /// 支持多种别名：Push/Pull, Upload/Download, LocalToRemote/RemoteToLocal 等
    /// </summary>
    public string Direction
    {
        get => _direction.ToString();
        set => _direction = SyncDirectionParser.Parse(value);
    }

    /// <summary>
    /// 获取解析后的同步方向枚举值
    /// </summary>
    public SyncDirection GetDirection() => _direction;
}
