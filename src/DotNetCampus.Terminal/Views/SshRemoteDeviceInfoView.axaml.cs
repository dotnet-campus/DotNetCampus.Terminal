using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Consolonia.Controls;
using DotNetCampus.Logging;
using DotNetCampus.Terminal.ViewModels;
using AvaloniaWindow = Avalonia.Controls.Window;
using ConsoloniaWindow = Consolonia.Controls.Window;

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
        // 取消之前的订阅
        if (sender is SshRemoteDeviceInfoView view && view.Tag is SshRemoteDeviceInfoViewModel oldViewModel)
        {
            oldViewModel.Commands.DeleteDeviceRequested -= OnDeleteDeviceRequested;
        }

        // 订阅新的ViewModel事件
        if (DataContext is SshRemoteDeviceInfoViewModel newViewModel)
        {
            Tag = newViewModel; // 保存引用以便取消订阅
            newViewModel.Commands.DeleteDeviceRequested += OnDeleteDeviceRequested;
        }
    }

    private async void OnDeleteDeviceRequested(object? sender, EventArgs e)
    {
        if (DataContext is not SshRemoteDeviceInfoViewModel viewModel)
            return;

        try
        {
            var deviceName = viewModel.ConnectionName;
            var message = $"确定要删除设备 '{deviceName}' 吗？\n\n此操作将永久删除该设备的配置信息，无法撤销。";

            // 显示确认对话框
            var result = await MessageBox.ShowDialog(
                this.GetVisualRoot() as ConsoloniaWindow,
                message,
                "删除设备",
                MessageBoxStyle.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                Log.Info($"[UI] 用户确认删除设备: {deviceName}");

                // 执行删除操作
                await viewModel.Commands.ExecuteDeleteAsync();

                Log.Info($"[UI] 设备删除完成: {deviceName}");

                // TODO: 通知主界面刷新设备列表或导航回主界面
            }
            else
            {
                Log.Info($"[UI] 用户取消删除设备: {deviceName}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[UI] 删除设备时发生错误: {ex.Message}", ex);

            // 显示错误消息
            await MessageBox.ShowDialog(
                this.GetVisualRoot() as ConsoloniaWindow,
                $"删除设备时发生错误：\n{ex.Message}",
                "删除失败",
                MessageBoxStyle.Ok);
        }
    }
}

