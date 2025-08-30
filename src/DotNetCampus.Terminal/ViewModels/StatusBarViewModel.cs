using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Logging;

namespace DotNetCampus.Terminal.ViewModels;

/// <summary>
/// 现代化状态栏ViewModel
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

    #region 现代化状态栏命令

    /// <summary>
    /// 刷新命令
    /// </summary>
    public AsyncCommand RefreshCommand { get; private set; } = null!;

    /// <summary>
    /// 显示帮助命令
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
        Log.Info("[StatusBar] 刷新设备列表");
        StatusTip.ShowOperationStatus("刷新设备列表", true);
        await _mainViewModel.ReloadDevicesCommand.ExecuteAsync();
        StatusTip.ShowOperationStatus("刷新设备列表");
    }

    private void OnShowHelp()
    {
        Log.Info("[StatusBar] 显示帮助");
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
            Log.Error($"[StatusBar] 打开GitHub仓库失败: {ex.Message}");
            StatusTip.ShowError("打开GitHub仓库失败，请手动访问: https://github.com/dotnet-campus/dotnetCampus.Terminal");
        }
    }

    #endregion
}
