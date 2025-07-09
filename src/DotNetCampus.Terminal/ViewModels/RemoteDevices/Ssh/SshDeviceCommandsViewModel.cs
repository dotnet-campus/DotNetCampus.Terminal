using DotNetCampus.Logging;
using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Modules.Configurations;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Utils;
using DotNetCampus.Terminal.Framework.DependencyInjection;

namespace DotNetCampus.Terminal.ViewModels.RemoteDevices.Ssh;

/// <summary>
/// SSH设备命令处理的ViewModel
/// </summary>
public partial record SshDeviceCommandsViewModel : BindableRecord
{
    private readonly SshDeviceSyncViewModel _syncViewModel;
    private readonly Func<SshRemoteDeviceInfo> _getDeviceInfo;
    private readonly SshRemoteDeviceInfoViewModel _owner;

    public SshDeviceCommandsViewModel(SshRemoteDeviceInfoViewModel owner, Func<SshRemoteDeviceInfo> getDeviceInfo)
    {
        _syncViewModel = owner.Sync;
        _getDeviceInfo = getDeviceInfo;
        _owner = owner;
        InitializeCommands();
    }

    /// <summary>
    /// 同步全部命令
    /// </summary>
    public AsyncCommand SyncAllCommand { get; private set; } = null!;

    /// <summary>
    /// 取消同步命令
    /// </summary>
    public AsyncCommand CancelSyncCommand { get; private set; } = null!;

    /// <summary>
    /// 全部启用命令
    /// </summary>
    public AsyncCommand EnableAllCommand { get; private set; } = null!;

    /// <summary>
    /// 全部禁用命令
    /// </summary>
    public AsyncCommand DisableAllCommand { get; private set; } = null!;

    /// <summary>
    /// 显示详细诊断信息的命令
    /// </summary>
    public ActionCommand ShowDiagnosticsCommand { get; private set; } = null!;

    /// <summary>
    /// 打开新Shell连接命令
    /// </summary>
    public AsyncCommand OpenShellCommand { get; private set; } = null!;

    /// <summary>
    /// 保存配置命令
    /// </summary>
    public AsyncCommand SaveConfigurationCommand { get; private set; } = null!;

    /// <summary>
    /// 初始化命令
    /// </summary>
    private void InitializeCommands()
    {
        SyncAllCommand = new AsyncCommand(OnSyncAllAsync);
        CancelSyncCommand = new AsyncCommand(OnCancelSyncAsync);
        EnableAllCommand = new AsyncCommand(OnEnableAllAsync);
        DisableAllCommand = new AsyncCommand(OnDisableAllAsync);
        ShowDiagnosticsCommand = new ActionCommand(OnShowDiagnostics);
        OpenShellCommand = new AsyncCommand(OnOpenShellAsync);
        SaveConfigurationCommand = new AsyncCommand(OnSaveConfigurationAsync);
    }

    /// <summary>
    /// 同步所有启用的目录
    /// </summary>
    private async Task OnSyncAllAsync()
    {
        var deviceInfo = _getDeviceInfo();
        await _syncViewModel.SyncAllAsync(deviceInfo);
    }

    /// <summary>
    /// 取消正在进行的同步
    /// </summary>
    private async Task OnCancelSyncAsync()
    {
        await _syncViewModel.CancelSyncAsync();
    }

    /// <summary>
    /// 启用所有同步组
    /// </summary>
    private async Task OnEnableAllAsync()
    {
        await _syncViewModel.EnableAllAsync();
    }

    /// <summary>
    /// 禁用所有同步组
    /// </summary>
    private async Task OnDisableAllAsync()
    {
        await _syncViewModel.DisableAllAsync();
    }

    /// <summary>
    /// 显示详细诊断信息
    /// </summary>
    private void OnShowDiagnostics()
    {
        _syncViewModel.ShowDiagnostics();
    }

    /// <summary>
    /// 打开新Shell连接
    /// </summary>
    private async Task OnOpenShellAsync()
    {
        var sshInfo = _getDeviceInfo();

        try
        {
            Log.Info($"[UI] 尝试在新标签页打开SSH连接: {sshInfo.UserName}@{sshInfo.Host}:{sshInfo.Port}");

            // 使用ShellUtils在新的终端标签页中打开SSH连接
            var success = await ShellUtils.OpenSshInNewTabAsync(
                sshInfo.Host,
                sshInfo.Port,
                sshInfo.UserName,
                sshInfo.Password);

            if (success)
            {
                Log.Info("[UI] 新SSH连接已在新标签页中打开");
            }
            else
            {
                Log.Warn("[UI] 无法在新标签页中打开SSH连接，可能需要手动连接");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[UI] 打开新Shell连接时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    private async Task OnSaveConfigurationAsync()
    {
        try
        {
            var currentDeviceInfo = _getDeviceInfo();
            Log.Info($"[UI] 开始保存设备配置: {currentDeviceInfo.ConnectionName}");

            // 获取配置管理器
            var configurationManager = Container.Current.EnsureGet<ConfigurationManager>();

            // 创建新的设备信息，包含更新的同步组配置
            var updatedDeviceInfo = currentDeviceInfo with
            {
                SyncGroups = _syncViewModel.GetSyncGroupConfigurations()
            };

            // 保存配置
            await configurationManager.SaveRemoteDeviceAsync(updatedDeviceInfo);

            // 更新父ViewModel的Info属性，确保数据同步
            _owner.UpdateInfo();

            Log.Info($"[UI] 设备配置保存成功并已同步列表数据: {currentDeviceInfo.ConnectionName}");
        }
        catch (Exception ex)
        {
            var deviceInfo = _getDeviceInfo();
            Log.Error($"[UI] 保存设备配置失败: {deviceInfo.ConnectionName}, 错误: {ex.Message}", ex);
            // TODO: 显示错误消息给用户
        }
    }
}
