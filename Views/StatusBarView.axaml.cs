using Avalonia.Controls;
using Avalonia.Input;
using DotNetCampus.Terminal.ViewModels;

namespace DotNetCampus.Terminal.Views;

public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 功能键按钮鼠标进入事件
    /// </summary>
    private void OnFunctionKeyEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Button button && 
            button.Tag is string description &&
            DataContext is StatusBarViewModel viewModel)
        {
            viewModel.StatusTip.ShowTip(description);
        }
    }

    /// <summary>
    /// 功能键按钮鼠标离开事件
    /// </summary>
    private void OnFunctionKeyExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is StatusBarViewModel viewModel)
        {
            viewModel.StatusTip.Reset();
        }
    }
}
