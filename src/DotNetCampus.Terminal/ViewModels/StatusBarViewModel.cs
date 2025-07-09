using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampus.Logging;

namespace DotNetCampus.Terminal.ViewModels;

/// <summary>
/// 状态栏ViewModel，提供全局功能键的命令绑定
/// </summary>
public record StatusBarViewModel : BindableRecord
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MainViewModel _mainViewModel;

    public StatusBarViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _mainViewModel = serviceProvider.EnsureGet<MainViewModel>();
        StatusTip = serviceProvider.EnsureGet<StatusTipViewModel>();
        
        InitializeCommands();
    }

    /// <summary>
    /// 状态提示ViewModel
    /// </summary>
    public StatusTipViewModel StatusTip { get; }

    #region 命令属性

    /// <summary>
    /// F1 - 显示帮助
    /// </summary>
    public ActionCommand ShowHelpCommand { get; private set; } = null!;

    /// <summary>
    /// F2 - 连接到当前选中的设备
    /// </summary>
    public ActionCommand ConnectCommand { get; private set; } = null!;

    /// <summary>
    /// F3 - 开始同步当前选中的设备
    /// </summary>
    public AsyncCommand StartSyncCommand { get; private set; } = null!;

    /// <summary>
    /// F4 - 创建新设备
    /// </summary>
    public ActionCommand NewDeviceCommand { get; private set; } = null!;

    /// <summary>
    /// F5 - 刷新设备列表
    /// </summary>
    public AsyncCommand RefreshCommand { get; private set; } = null!;

    /// <summary>
    /// F6 - 保存当前配置
    /// </summary>
    public AsyncCommand SaveConfigCommand { get; private set; } = null!;

    /// <summary>
    /// F7 - 打开终端到当前选中的设备
    /// </summary>
    public AsyncCommand OpenShellCommand { get; private set; } = null!;

    /// <summary>
    /// F8 - 切换搜索模式
    /// </summary>
    public ActionCommand ToggleSearchCommand { get; private set; } = null!;

    /// <summary>
    /// F9 - 打开设置
    /// </summary>
    public ActionCommand SettingsCommand { get; private set; } = null!;

    /// <summary>
    /// F10 - 退出应用程序
    /// </summary>
    public ActionCommand ExitCommand { get; private set; } = null!;

    #endregion

    #region 私有方法

    private void InitializeCommands()
    {
        ShowHelpCommand = new ActionCommand(OnShowHelp);
        ConnectCommand = new ActionCommand(OnConnect);
        StartSyncCommand = new AsyncCommand(OnStartSyncAsync);
        NewDeviceCommand = new ActionCommand(OnNewDevice);
        RefreshCommand = new AsyncCommand(OnRefreshAsync);
        SaveConfigCommand = new AsyncCommand(OnSaveConfigAsync);
        OpenShellCommand = new AsyncCommand(OnOpenShellAsync);
        ToggleSearchCommand = new ActionCommand(OnToggleSearch);
        SettingsCommand = new ActionCommand(OnSettings);
        ExitCommand = new ActionCommand(OnExit);
    }

    private void OnShowHelp()
    {
        Log.Info("[StatusBar] F1 - 显示帮助");
        StatusTip.ShowTip("F1 - 显示帮助文档和操作指南 (功能开发中)");
        // TODO: 实现帮助界面或帮助文档显示
    }

    private void OnConnect()
    {
        Log.Info("[StatusBar] F2 - 连接到设备");
        StatusTip.ShowOperationStatus("测试连接", true);
        // TODO: 连接到当前选中的设备
        // 需要从MainViewModel获取当前选中的设备并执行连接
    }

    private async Task OnStartSyncAsync()
    {
        Log.Info("[StatusBar] F3 - 开始同步");
        StatusTip.ShowOperationStatus("同步", true);
        // TODO: 对当前选中的设备执行同步操作
        // 需要从MainViewModel获取当前选中的设备并执行同步
    }

    private void OnNewDevice()
    {
        Log.Info("[StatusBar] F4 - 创建新设备");
        StatusTip.ShowTip("F4 - 切换到创建新设备界面");
        // TODO: 切换到创建新设备界面
        // 可以通过MainViewModel选择"创建新设备"节点
    }

    private async Task OnRefreshAsync()
    {
        Log.Info("[StatusBar] F5 - 刷新设备列表");
        StatusTip.ShowOperationStatus("刷新设备列表", true);
        await _mainViewModel.ReloadDevicesCommand.ExecuteAsync();
        StatusTip.ShowOperationStatus("刷新设备列表");
    }

    private async Task OnSaveConfigAsync()
    {
        Log.Info("[StatusBar] F6 - 保存配置");
        StatusTip.ShowOperationStatus("保存配置", true);
        // TODO: 保存当前选中设备的配置
        // 需要从MainViewModel获取当前选中的设备并执行保存
    }

    private async Task OnOpenShellAsync()
    {
        Log.Info("[StatusBar] F7 - 打开终端");
        StatusTip.ShowOperationStatus("打开SSH终端", true);
        // TODO: 打开终端到当前选中的设备
        // 需要从MainViewModel获取当前选中的设备并打开Shell
    }

    private void OnToggleSearch()
    {
        Log.Info("[StatusBar] F8 - 切换搜索模式");
        StatusTip.ShowTip("F8 - 切换搜索框焦点 (功能开发中)");
        // TODO: 切换搜索框的焦点或显示/隐藏搜索功能
    }

    private void OnSettings()
    {
        Log.Info("[StatusBar] F9 - 打开设置");
        StatusTip.ShowTip("F9 - 打开应用程序设置 (功能开发中)");
        // TODO: 打开应用程序设置界面
    }

    private void OnExit()
    {
        Log.Info("[StatusBar] F10 - 退出应用程序");
        StatusTip.ShowTip("F10 - 正在安全退出应用程序...");
        // TODO: 安全退出应用程序
        Environment.Exit(0);
    }

    #endregion
}
