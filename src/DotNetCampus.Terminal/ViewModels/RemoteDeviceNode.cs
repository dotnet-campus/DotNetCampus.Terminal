using Avalonia.Collections;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Modules.Configurations;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Utils;
using Avalonia.Threading;

namespace DotNetCampus.Terminal.ViewModels;

public interface IRemoteDeviceNode
{
    IReadOnlyList<IRemoteDeviceNode> Children { get; }

    public static RemoteDeviceGroupNode From(RemoteDeviceGroup group)
    {
        return new RemoteDeviceGroupNode(group)
        {
            Children = group.Devices.Select(From).ToList(),
        };
    }

    public static RemoteDeviceInfoNode From(IRemoteDeviceInfo info)
    {
        return info switch
        {
            SshRemoteDeviceInfo sshInfo => new SshRemoteDeviceInfoNode(sshInfo),
            _ => new FallbackRemoteDeviceInfoNode(info),
        };
    }
}

public record CreateNewRemoteDeviceNode : IRemoteDeviceNode
{
    public IReadOnlyList<IRemoteDeviceNode> Children { get; } = [];
}

public record FavoriteDeviceGroupNode : IRemoteDeviceNode
{
    IReadOnlyList<IRemoteDeviceNode> IRemoteDeviceNode.Children => Children;

    public AvaloniaList<IRemoteDeviceNode> Children { get; init; } = [];
}

public record RemoteDeviceGroupNode(RemoteDeviceGroup Info) : IRemoteDeviceNode
{
    public required IReadOnlyList<IRemoteDeviceNode> Children { get; init; }
}

public abstract record RemoteDeviceInfoNode(IRemoteDeviceInfo Info) : BindableRecord, IRemoteDeviceNode
{
    private ConnectionState _connectionState;

    public ConnectionState ConnectionState
    {
        get => _connectionState;
        protected set => SetField(ref _connectionState, value);
    }

    public IReadOnlyList<IRemoteDeviceNode> Children { get; } = [];

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ConnectionState = ConnectionState.Testing;
            });

            var state = await OnTestConnectionAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ConnectionState = state ? ConnectionState.Online : ConnectionState.Offline;
            });

            return state;
        }
        catch (Exception)
        {
            // 确保即使出现异常，也要更新UI状态为离线
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ConnectionState = ConnectionState.Offline;
            });

            return false;
        }
    }

    protected abstract Task<bool> OnTestConnectionAsync();
}

public record SshRemoteDeviceInfoNode : RemoteDeviceInfoNode
{
    public SshRemoteDeviceInfoNode(SshRemoteDeviceInfo info) : base(info)
    {
    }

    protected override async Task<bool> OnTestConnectionAsync()
    {
        var sshInfo = (SshRemoteDeviceInfo)Info;

        // 使用NetworkUtils工具类进行TCP连接测试
        return await NetworkUtils.TestTcpConnectionAsync(sshInfo.HostName, sshInfo.Port);
    }
}

public sealed record FallbackRemoteDeviceInfoNode : RemoteDeviceInfoNode
{
    public FallbackRemoteDeviceInfoNode(IRemoteDeviceInfo info) : base(info)
    {
    }

    protected override Task<bool> OnTestConnectionAsync()
    {
        // 对于通用的远程设备信息，暂时返回false
        // 后续可以根据具体的设备类型实现相应的连接测试逻辑
        return Task.FromResult(false);
    }
}
