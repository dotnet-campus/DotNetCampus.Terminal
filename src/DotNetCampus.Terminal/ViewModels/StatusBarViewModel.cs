using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Logging;

namespace DotNetCampus.Terminal.ViewModels;

/// <summary>
/// 现代化状态栏ViewModel - 提供应用状态信息和快捷操作
/// 遵循现代GUI应用设计模式，摒弃传统TUI功能键风格
/// </summary>
public record StatusBarViewModel : BindableRecord
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MainViewModel _mainViewModel;

    public StatusBarViewModel(MainViewModel owner, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _mainViewModel = owner;
        StatusTip = owner.StatusTip;

        InitializeCommands();
    }

    /// <summary>
    /// 状态提示ViewModel
    /// </summary>
    public StatusTipViewModel StatusTip { get; }

    #region 现代化状态栏操作

    /// <summary>
    /// 刷新设备列表操作
    /// </summary>
    public AsyncCommand RefreshCommand { get; private set; } = null!;

    /// <summary>
    /// 打开帮助文档操作
    /// </summary>
    public ActionCommand ShowHelpCommand { get; private set; } = null!;

    #endregion

    #region 私有方法

    private void InitializeCommands()
    {
        RefreshCommand = new AsyncCommand(OnRefreshAsync);
        ShowHelpCommand = new ActionCommand(OnShowHelp);
    }

    private async Task OnRefreshAsync()
    {
        Log.Info("[UI] 用户触发设备列表刷新操作");
        StatusTip.ShowOperationStatus("正在刷新设备列表...", true);
        await _mainViewModel.ReloadDevicesCommand.ExecuteAsync();
        StatusTip.ShowOperationStatus("设备列表刷新完成");
    }

    private void OnShowHelp()
    {
        Log.Info("[UI] 用户请求查看帮助文档");
        StatusTip.ShowTip("正在打开GitHub仓库...");

        try
        {
            // 在默认浏览器中打开GitHub仓库
            var url = "https://github.com/dotnet-campus/dotnetCampus.Terminal";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });

            StatusTip.ShowTip("已在浏览器中打开GitHub仓库");
        }
        catch (Exception ex)
        {
            Log.Error($"[UI] 打开GitHub仓库失败: {ex.Message}");
            StatusTip.ShowError("打开帮助文档失败，请手动访问: https://github.com/dotnet-campus/dotnetCampus.Terminal");
        }
    }

    #endregion
}
