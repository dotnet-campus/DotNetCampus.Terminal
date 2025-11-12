using Avalonia.Controls;
using DotNetCampus.Logging;
using DotNetCampus.Terminal.ViewModels;
using DotNetCampus.Terminal.ViewModels.RemoteDevices.Ssh;

namespace DotNetCampus.Terminal.Views;

public partial class SshRemoteDeviceInfoView : UserControl
{
    public SshRemoteDeviceInfoView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not SshRemoteDeviceInfoViewModel vm)
        {
            return;
        }

        vm.Commands.DeleteDeviceCommand.ProvideInteraction(new DeleteDeviceInteraction
        {
            ConfirmDeleteAsync = ConfirmDeleteAsync,
        }).WhenErrorOccurred(OnDeleteDeviceFailed);
    }

    private async Task<bool> ConfirmDeleteAsync()
    {
        var vm = (SshRemoteDeviceInfoViewModel)DataContext!;

        var deviceName = vm.ConnectionName;
        var message = $"确定要删除设备 '{deviceName}' 吗？\n\n此操作将永久删除该设备的配置信息，无法撤销。";

        // 显示确认对话框
        // var result = await MessageBox.ShowDialog(
        //     this,
        //     "删除设备",
        //     message,
        //     MessageBoxStyle.YesNo);

        // return result is MessageBoxResult.Yes;

        return true;
    }

    private void OnDeleteDeviceFailed(Exception ex)
    {
        Log.Error($"[UI] 删除设备时发生错误: {ex.Message}", ex);

        // 显示错误消息
        // _ = MessageBox.ShowDialog(
        //     this,
        //     "删除失败",
        //     $"删除设备时发生错误：\n{ex.Message}",
        //     MessageBoxStyle.Ok);
    }
}
