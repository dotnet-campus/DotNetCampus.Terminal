using Avalonia.Collections;
using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Modules.Configurations;

namespace DotNetCampus.Terminal.ViewModels;

public class MainViewModel
{
    private readonly ConfigurationManager _configurationManager;

    public MainViewModel(IServiceProvider serviceProvider)
    {
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
        var remoteDevices = await _configurationManager.FetchRemoteDevicesAsync();
        foreach (var group in remoteDevices)
        {
            RemoteDevices.Add(IRemoteDeviceNode.From(group));
        }
    }
}
