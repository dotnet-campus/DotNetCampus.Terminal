using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Utils;
using Avalonia.Collections;

namespace DotNetCampus.Terminal.ViewModels;

public record SshRemoteDeviceInfoViewModel : RemoteDeviceInfoNode
{
    private string _connectionName;
    private string _host;
    private int _port;
    private string _userName;
    private string? _password;
    private SyncGroupViewModel? _selectedSyncGroup;

    public SshRemoteDeviceInfoViewModel() : base(new SshRemoteDeviceInfo
    {
        ConnectionName = "设计时设备",
        Host = "localhost",
        Port = 22,
        UserName = "username",
        Password = "password",
        SyncGroups = [
            new SyncGroupConfiguration
            {
                Name = "设计时项目",
                RemotePath = "/home/user/projects/design",
                LocalPath = @"D:\Projects\Design",
                Enabled = true
            },
            new SyncGroupConfiguration
            {
                Name = "设计时文档",
                RemotePath = "/home/user/documents",
                LocalPath = @"D:\Documents",
                Enabled = false
            }
        ]
    })
    {
        _connectionName = "设计时设备";
        _host = "localhost";
        _port = 22;
        _userName = "username";
        _password = "password";

        // 添加设计时示例数据
        SyncGroups.Add(new SyncGroupViewModel
        {
            Name = "设计时项目",
            RemotePath = "/home/user/projects/design",
            LocalPath = @"D:\Projects\Design",
            Status = SyncGroupStatus.Normal
        });

        SyncGroups.Add(new SyncGroupViewModel
        {
            Name = "设计时文档",
            RemotePath = "/home/user/documents",
            LocalPath = @"D:\Documents",
            Status = SyncGroupStatus.Disabled
        });
    }

    public SshRemoteDeviceInfoViewModel(SshRemoteDeviceInfo info) : base(info)
    {
        _connectionName = info.ConnectionName;
        _host = info.Host;
        _port = info.Port;
        _userName = info.UserName;
        _password = info.Password;

        // 从配置中加载同步组数据
        foreach (var syncGroup in info.SyncGroups)
        {
            SyncGroups.Add(new SyncGroupViewModel
            {
                Name = syncGroup.Name,
                RemotePath = syncGroup.RemotePath,
                LocalPath = syncGroup.LocalPath,
                Status = syncGroup.Enabled ? SyncGroupStatus.Normal : SyncGroupStatus.Disabled
            });
        }

        // 如果没有配置同步组，添加一个示例（仅用于演示）
        if (SyncGroups.Count == 0)
        {
            SyncGroups.Add(new SyncGroupViewModel
            {
                Name = "示例同步组",
                RemotePath = "/home/user/example",
                LocalPath = @"D:\Example",
                Status = SyncGroupStatus.Normal
            });
        }
    }

    /// <summary>
    /// 同步组列表
    /// </summary>
    public AvaloniaList<SyncGroupViewModel> SyncGroups { get; } = new();

    /// <summary>
    /// 选中的同步组
    /// </summary>
    public SyncGroupViewModel? SelectedSyncGroup
    {
        get => _selectedSyncGroup;
        set => SetField(ref _selectedSyncGroup, value);
    }

    public string ConnectionName
    {
        get => _connectionName;
        set => SetField(ref _connectionName, value);
    }

    public string Host
    {
        get => _host;
        set
        {
            if (SetField(ref _host, value))
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
            if (SetField(ref _port, value))
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
        set => SetField(ref _userName, value);
    }

    public string? Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    protected override async Task<bool> OnTestConnectionAsync()
    {
        var sshInfo = (SshRemoteDeviceInfo)Info;

        // 使用NetworkUtils工具类进行TCP连接测试
        return await NetworkUtils.TestTcpConnectionAsync(sshInfo.Host, sshInfo.Port);
    }
}
