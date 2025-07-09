using DotNetCampus.Terminal.FileSync;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Utils;
using DotNetCampus.Terminal.ViewModels.RemoteDevices.Ssh;
using DotNetCampus.Terminal.Framework.DependencyInjection;

namespace DotNetCampus.Terminal.ViewModels;

/// <summary>
/// SSH远程设备信息ViewModel - 重构后的精简版本
/// </summary>
public record SshRemoteDeviceInfoViewModel : RemoteDeviceInfoNode
{
    private string _localId;
    private string? _remoteId;
    private string _connectionName;
    private string _host;
    private int _port;
    private string _userName;
    private string? _password;

    public IServiceProvider ServiceProvider { get; }

    // 组合的子ViewModels - 使用简洁的属性名
    public SshDeviceSyncViewModel Sync { get; }
    public SshDeviceCommandsViewModel Commands { get; }

    /// <summary>
    /// 生成本地唯一标识符
    /// </summary>
    private static string GenerateLocalId()
    {
        return "device_" + Guid.NewGuid().ToString("N")[..16];
    }

    [Obsolete("仅供设计器使用", true)]
    public SshRemoteDeviceInfoViewModel() : this(null!, new SshRemoteDeviceInfo
    {
        LocalId = GenerateLocalId(),
        RemoteId = null,
        ConnectionName = "设计时设备",
        Host = "localhost",
        Port = 22,
        UserName = "username",
        Password = "password",
        SyncGroups =
        [
            new SyncGroupConfiguration
            {
                Name = "设计时项目",
                RemotePath = "/home/user/projects/design",
                LocalPath = @"D:\Projects\Design",
                Enabled = true,
            },
            new SyncGroupConfiguration
            {
                Name = "设计时文档",
                RemotePath = "/home/user/documents",
                LocalPath = @"D:\Documents",
                Enabled = false,
            },
        ],
    })
    {
    }

    public SshRemoteDeviceInfoViewModel(IServiceProvider serviceProvider, SshRemoteDeviceInfo info) : base(info)
    {
        ServiceProvider = serviceProvider;
        _localId = info.LocalId;
        _remoteId = info.RemoteId;
        _connectionName = info.ConnectionName;
        _host = info.Host;
        _port = info.Port;
        _userName = info.UserName;
        _password = info.Password;

        // 初始化子ViewModels
        var fileSyncService = Container.Current.EnsureGet<IFileSyncService>();
        Sync = new SshDeviceSyncViewModel(fileSyncService);
        Commands = new SshDeviceCommandsViewModel(this, GetCurrentDeviceInfo);

        // 从配置中加载同步组数据
        Sync.InitializeSyncGroups(info.SyncGroups);
    }

    #region 基础属性

    /// <summary>
    /// 本地唯一标识符
    /// </summary>
    public string LocalId
    {
        get => _localId;
        set => SetFieldTrackingChanges(ref _localId, value);
    }

    /// <summary>
    /// 远程设备唯一标识符
    /// </summary>
    public string? RemoteId
    {
        get => _remoteId;
        set => SetFieldTrackingChanges(ref _remoteId, value);
    }

    public string ConnectionName
    {
        get => _connectionName;
        set => SetFieldTrackingChanges(ref _connectionName, value);
    }

    public string Host
    {
        get => _host;
        set
        {
            if (SetFieldTrackingChanges(ref _host, value))
            {
                OnPropertyChanged(nameof(Address));
            }
        }
    }

    public int Port
    {
        get => _port;
        set
        {
            if (SetFieldTrackingChanges(ref _port, value))
            {
                OnPropertyChanged(nameof(Address));
            }
        }
    }

    public string Address => Port is 22
        ? Host
        : $"{Host}:{Port}";

    public string UserName
    {
        get => _userName;
        set => SetFieldTrackingChanges(ref _userName, value);
    }

    public string? Password
    {
        get => _password;
        set => SetFieldTrackingChanges(ref _password, value);
    }

    #endregion

    /// <summary>
    /// 更新基类的Info属性，确保与当前属性值同步
    /// </summary>
    public void UpdateInfo()
    {
        var currentInfo = GetCurrentDeviceInfo();
        Info = currentInfo;

        // 重置变更跟踪状态
        ResetChangeTracking();
    }

    /// <summary>
    /// 获取当前设备信息
    /// </summary>
    private SshRemoteDeviceInfo GetCurrentDeviceInfo()
    {
        return new SshRemoteDeviceInfo
        {
            LocalId = LocalId,
            RemoteId = RemoteId,
            ConnectionName = ConnectionName,
            Host = Host,
            Port = Port,
            UserName = UserName,
            Password = Password,
            SyncGroups = Sync.GetSyncGroupConfigurations(),
        };
    }

    protected override async Task<bool> OnTestConnectionAsync()
    {
        var sshInfo = GetCurrentDeviceInfo();

        // 使用NetworkUtils工具类进行TCP连接测试
        return await NetworkUtils.TestTcpConnectionAsync(sshInfo.Host, sshInfo.Port);
    }
}
