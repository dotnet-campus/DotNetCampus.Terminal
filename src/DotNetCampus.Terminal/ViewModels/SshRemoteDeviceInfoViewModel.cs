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
            Name = "MyVeryLongProjectName",
            RemotePath = "/home/user/projects/myproject/src/main/java/com/example/...",
            LocalPath = @"D:\Projects\MyVeryLongProjectName\src\main\java\com\example\...",
            Status = SyncGroupStatus.Normal
        });

        SyncGroups.Add(new SyncGroupViewModel
        {
            Name = "VeryLongDepartment",
            RemotePath = "/home/user/documents/work/department/reports/...",
            LocalPath = @"D:\Documents\Work\VeryLongDepartment\reports\...",
            Status = SyncGroupStatus.Error
        });
    }

    public SshRemoteDeviceInfoViewModel(SshRemoteDeviceInfo info) : base(info)
    {
        _connectionName = info.ConnectionName;
        _host = info.Host;
        _port = info.Port;
        _userName = info.UserName;
        _password = info.Password;

        // 添加设计时示例数据
        SyncGroups.Add(new SyncGroupViewModel
        {
            Name = "MyVeryLongProjectName",
            RemotePath = "/home/user/projects/myproject/src/main/java/com/example/...",
            LocalPath = @"D:\Projects\MyVeryLongProjectName\src\main\java\com\example\...",
            Status = SyncGroupStatus.Normal
        });

        SyncGroups.Add(new SyncGroupViewModel
        {
            Name = "VeryLongDepartment",
            RemotePath = "/home/user/documents/work/department/reports/...",
            LocalPath = @"D:\Documents\Work\VeryLongDepartment\reports\...",
            Status = SyncGroupStatus.Error
        });
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
