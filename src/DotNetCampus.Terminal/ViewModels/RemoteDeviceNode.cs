using Avalonia.Collections;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Modules.Configurations;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using Avalonia.Threading;
using System.Threading;

namespace DotNetCampus.Terminal.ViewModels;

public interface IRemoteDeviceNode
{
    IReadOnlyList<IRemoteDeviceNode> Children { get; }

    public static RemoteDeviceGroupNode From(IServiceProvider serviceProvider, RemoteDeviceGroup group)
    {
        return new RemoteDeviceGroupNode(group)
        {
            Children = group.Devices.Select(x => From(serviceProvider, x)).ToList(),
        };
    }

    public static RemoteDeviceInfoNode From(IServiceProvider serviceProvider, IRemoteDeviceInfo info)
    {
        return info switch
        {
            SshRemoteDeviceInfo sshInfo => new SshRemoteDeviceInfoViewModel(serviceProvider, sshInfo),
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

public record RemoteDeviceGroupNode(RemoteDeviceGroup Info) : TrackingUnsavedBindableRecord, IRemoteDeviceNode
{
    private int _onlineCount;

    public int OnlineCount
    {
        get => _onlineCount;
        set => SetField(ref _onlineCount, value);
    }

    public required IReadOnlyList<IRemoteDeviceNode> Children { get; init; }

    public async Task TestConnectionAsync()
    {
        OnlineCount = 0;

        var devices = Children.OfType<RemoteDeviceInfoNode>().ToList();
        var tasks = devices.Select(async device =>
        {
            var result = await device.TestConnectionAsync();
            if (result)
            {
                // 每个连接测试成功后立即更新计数
                Interlocked.Increment(ref _onlineCount);
                _ = Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // 确保在UI线程上更新在线设备计数
                    OnPropertyChanged(nameof(OnlineCount));
                });
            }
            return result;
        });

        await Task.WhenAll(tasks);
    }
}

public abstract record RemoteDeviceInfoNode : TrackingUnsavedBindableRecord, IRemoteDeviceNode
{
    private IRemoteDeviceInfo _info;
    private ConnectionState _connectionState;

    protected RemoteDeviceInfoNode(IRemoteDeviceInfo info)
    {
        _info = info;
    }

    /// <summary>
    /// 设备信息
    /// </summary>
    public IRemoteDeviceInfo Info
    {
        get => _info;
        protected set => SetFieldAndUnsaved(ref _info, value);
    }

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
