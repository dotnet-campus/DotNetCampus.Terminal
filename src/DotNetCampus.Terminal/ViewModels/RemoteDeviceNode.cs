using Avalonia.Collections;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Modules.Configurations;
using DotNetCampus.Terminal.Modules.Configurations.Models;

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
            _ => new RemoteDeviceInfoNode(info),
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
        ConnectionState = ConnectionState.Testing;
        var state = await OnTestConnectionAsync();
        ConnectionState = state ? ConnectionState.Online : ConnectionState.Offline;
        return state;
    }

    protected abstract Task<bool> OnTestConnectionAsync();
}

public record SshRemoteDeviceInfoNode : RemoteDeviceInfoNode
{
    public SshRemoteDeviceInfoNode(SshRemoteDeviceInfo info) : base(info)
    {
    }

    protected override Task<bool> OnTestConnectionAsync()
    {
        var sshInfo = (SshRemoteDeviceInfo)Info;
    }
}
