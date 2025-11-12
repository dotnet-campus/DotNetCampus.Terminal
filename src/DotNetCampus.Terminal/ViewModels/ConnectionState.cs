namespace DotNetCampus.Terminal.ViewModels;

/// <summary>
/// 设备连接状态。
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// 正在测试。
    /// </summary>
    Testing,

    /// <summary>
    /// 设备在线。
    /// </summary>
    Online,

    /// <summary>
    /// 设备已离线。
    /// </summary>
    Offline,
}
