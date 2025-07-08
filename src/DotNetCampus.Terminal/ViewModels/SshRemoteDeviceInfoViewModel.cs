using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Utils;

namespace DotNetCampus.Terminal.ViewModels;

public record SshRemoteDeviceInfoViewModel : RemoteDeviceInfoNode
{
    private string _connectionName;
    private string _host;
    private int _port;
    private string _userName;
    private string? _password;

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
    }

    public SshRemoteDeviceInfoViewModel(SshRemoteDeviceInfo info) : base(info)
    {
        _connectionName = info.ConnectionName;
        _host = info.Host;
        _port = info.Port;
        _userName = info.UserName;
        _password = info.Password;
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
