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
    /// 处理功能键按钮的鼠标悬停事件
    /// </summary>
    private void OnFunctionKeyHover(object? sender, PointerEventArgs e)
    {
        if (sender is Button button && 
            button.Tag is string tag && 
            DataContext is StatusBarViewModel viewModel)
        {
            // Tag格式: "F1,显示帮助文档和操作指南,True"
            var parts = tag.Split(',');
            if (parts.Length == 3)
            {
                var functionKey = parts[0];
                var description = parts[1];
                var isEnabled = bool.Parse(parts[2]);
                
                viewModel.OnFunctionKeyHover(functionKey, description, isEnabled);
            }
        }
    }

    /// <summary>
    /// 处理鼠标离开功能键按钮事件
    /// </summary>
    private void OnFunctionKeyLeave(object? sender, PointerEventArgs e)
    {
        if (DataContext is StatusBarViewModel viewModel)
        {
            viewModel.OnFunctionKeyLeave();
        }
    }
}
