namespace DotNetCampus.Terminal.Modules.Configurations.Models;

/// <summary>
/// TOML 设备配置文件根对象
/// </summary>
public class TomlDeviceConfiguration
{
    /// <summary>
    /// SSH 设备配置列表
    /// </summary>
    public List<SshDeviceConfiguration> SshDevices { get; set; } = new();
}

/// <summary>
/// SSH 设备配置
/// </summary>
public class SshDeviceConfiguration
{
    /// <summary>
    /// 连接名称
    /// </summary>
    public string ConnectionName { get; set; } = string.Empty;

    /// <summary>
    /// 主机地址
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// 端口号
    /// </summary>
    public int Port { get; set; } = 22;

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 密码（可选）
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 同步组配置列表
    /// </summary>
    public List<SyncGroupConfiguration> SyncGroups { get; set; } = new();
}

/// <summary>
/// 同步组配置
/// </summary>
public class SyncGroupConfiguration
{
    /// <summary>
    /// 同步组名称
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
}
