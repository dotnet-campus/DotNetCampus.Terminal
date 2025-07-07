using Avalonia.Collections;
using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Modules.Configurations;
using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.ViewModels;

public class MainViewModel
{
    private readonly ConfigurationManager _configurationManager;

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _configurationManager = serviceProvider.EnsureGet<ConfigurationManager>();
        ReloadDevicesCommand = new AsyncCommand(OnReloadDevices);
    }

    public AsyncCommand ReloadDevicesCommand { get; }

    public AvaloniaList<IRemoteDeviceInfo> RemoteDevices { get; } = [];

    private async Task OnReloadDevices()
    {
        var remoteDevices = await _configurationManager.FetchRemoteDevicesAsync();
        foreach (var group in remoteDevices)
        {
            foreach (var device in group.Devices)
            {
                RemoteDevices.Add(device);
            }
        }
    }
}
