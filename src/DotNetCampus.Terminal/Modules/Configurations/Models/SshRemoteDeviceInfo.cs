namespace DotNetCampus.Terminal.Modules.Configurations.Models;

/// <summary>
/// 表示一个通过 SSH 协议连接的远程设备信息。
/// </summary>
public record SshRemoteDeviceInfo : IRemoteDeviceInfo
{
    public required string ConnectionName { get; init; }

    public RemoteDeviceType DeviceType { get; } = RemoteDeviceType.LinuxSsh;

    /// <summary>
    /// 远程设备主机名或 IP 地址。
    /// </summary>
    public required string HostName { get; init; }

    /// <summary>
    /// 远程设备端口号。
    /// </summary>
    public required int Port { get; init; }

    /// <summary>
    /// 远程连接所使用的用户名。
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// 远程连接所使用的密码。
    /// </summary>
    public string? Password { get; init; }
}

/// <summary>
/// 远程设备信息接口。
/// </summary>
public interface IRemoteDeviceInfo
{
    /// <summary>
    /// 连接名。
    /// </summary>
    string ConnectionName { get; }

    /// <summary>
    /// 远程设备类型。
    /// </summary>
    RemoteDeviceType DeviceType { get; }
}

/// <summary>
/// 远程设备类型。不同的远程设备类型有不同的连接方式。
/// </summary>
public enum RemoteDeviceType
{
    /// <summary>
    /// 使用 SSH 协议连接的远程设备。
    /// </summary>
    LinuxSsh,

    /// <summary>
    /// 对等设备，即目标设备上已安装本应用并已启用对等连接功能。
    /// </summary>
    PeerToPeer,
}
