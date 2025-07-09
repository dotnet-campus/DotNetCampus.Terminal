using Avalonia.Collections;
using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Modules.Configurations;

namespace DotNetCampus.Terminal.ViewModels;

public class MainViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigurationManager _configurationManager;

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _configurationManager = serviceProvider.EnsureGet<ConfigurationManager>();

        RemoteDevices.Add(new CreateNewRemoteDeviceNode());
        RemoteDevices.Add(new FavoriteDeviceGroupNode());

        ReloadDevicesCommand = new AsyncCommand(OnReloadDevices);
    }

    public AsyncCommand ReloadDevicesCommand { get; }

    public AvaloniaList<IRemoteDeviceNode> RemoteDevices { get; } = [];

    public AvaloniaList<IRemoteDeviceNode> FavoriteDevices => ((FavoriteDeviceGroupNode)RemoteDevices[1]).Children;

    private async Task OnReloadDevices()
    {
        // 清理旧的设备组，但保留前两个固定节点（创建新设备和收藏夹）
        for (var i = RemoteDevices.Count - 1; i >= 2; i--)
        {
            RemoteDevices.RemoveAt(i);
        }

        // 重新加载远程设备配置
        var remoteDevices = await _configurationManager.FetchRemoteDevicesAsync();
        foreach (var group in remoteDevices)
        {
            var deviceGroupNode = IRemoteDeviceNode.From(_serviceProvider, group);
            RemoteDevices.Add(deviceGroupNode);
        }

        // 重新测试设备在线状态
        await TestConnectionAsync();
    }

    public async Task TestConnectionAsync()
    {
        var tasks = RemoteDevices.Skip(2)
            .OfType<RemoteDeviceGroupNode>()
            .Select(device => Task.Run(async () =>
            {
                await device.TestConnectionAsync();
            }))
            .ToList();
        await Task.WhenAll(tasks);
    }
}
