using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Modules.SshManagement;
using DotNetCampus.Logging;

namespace DotNetCampus.Terminal.ViewModels.RemoteDevices.Ssh;

/// <summary>
/// SSH设备部署相关的ViewModel - 专门处理SSH密钥部署功能
/// </summary>
public record SshDeviceDeployViewModel : BindableRecord
{
    private readonly Func<SshRemoteDeviceInfo> _getDeviceInfo;
    private readonly SshKeyDeploymentService _deploymentService;

    private bool _isDeploying;
    private string _currentStep = string.Empty;
    private int _progressPercent;
    private bool _hasError;
    private string _errorMessage = string.Empty;
    private bool _canRetry;
    private bool _canRollback;
    private bool _confirmOperation;
    private bool _disablePasswordAuth;
    private string _passphrase = string.Empty;

    public SshDeviceDeployViewModel(Func<SshRemoteDeviceInfo> getDeviceInfo)
    {
        _getDeviceInfo = getDeviceInfo ?? throw new ArgumentNullException(nameof(getDeviceInfo));
        _deploymentService = new SshKeyDeploymentService();

        // 初始化命令
        DeployKeyCommand = new AsyncCommand(DeployKeyAsync);
        RetryDeployCommand = new AsyncCommand(DeployKeyAsync);
        RollbackCommand = new AsyncCommand(RollbackAsync);
        ClearErrorCommand = new ActionCommand(ClearError);

        // 初始化命令状态
        UpdateCommandStates();
    }

    #region 部署状态属性

    /// <summary>
    /// 是否正在部署
    /// </summary>
    public bool IsDeploying
    {
        get => _isDeploying;
        private set
        {
            if (SetField(ref _isDeploying, value))
            {
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 当前执行步骤
    /// </summary>
    public string CurrentStep
    {
        get => _currentStep;
        private set => SetField(ref _currentStep, value);
    }

    /// <summary>
    /// 进度百分比 (0-100)
    /// </summary>
    public int ProgressPercent
    {
        get => _progressPercent;
        private set => SetField(ref _progressPercent, value);
    }

    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        private set
        {
            if (SetField(ref _hasError, value))
            {
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetField(ref _errorMessage, value);
    }

    /// <summary>
    /// 是否可以重试
    /// </summary>
    public bool CanRetry
    {
        get => _canRetry;
        private set
        {
            if (SetField(ref _canRetry, value))
            {
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 是否可以回滚
    /// </summary>
    public bool CanRollback
    {
        get => _canRollback;
        private set
        {
            if (SetField(ref _canRollback, value))
            {
                UpdateCommandStates();
            }
        }
    }

    #endregion

    #region 用户输入属性

    /// <summary>
    /// 用户确认操作
    /// </summary>
    public bool ConfirmOperation
    {
        get => _confirmOperation;
        set
        {
            if (SetField(ref _confirmOperation, value))
            {
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 禁用密码认证（提高安全性）
    /// </summary>
    public bool DisablePasswordAuth
    {
        get => _disablePasswordAuth;
        set => SetField(ref _disablePasswordAuth, value);
    }

    /// <summary>
    /// SSH密钥密码短语（可选）
    /// </summary>
    public string Passphrase
    {
        get => _passphrase;
        set => SetField(ref _passphrase, value);
    }

    #endregion

    #region 命令

    /// <summary>
    /// 部署SSH密钥命令
    /// </summary>
    public AsyncCommand DeployKeyCommand { get; }

    /// <summary>
    /// 重试部署命令
    /// </summary>
    public AsyncCommand RetryDeployCommand { get; }

    /// <summary>
    /// 回滚命令
    /// </summary>
    public AsyncCommand RollbackCommand { get; }

    /// <summary>
    /// 清除错误命令
    /// </summary>
    public ActionCommand ClearErrorCommand { get; }

    #endregion

    #region 命令实现

    /// <summary>
    /// 执行SSH密钥部署
    /// </summary>
    private async Task DeployKeyAsync()
    {
        if (!ConfirmOperation)
        {
            ErrorMessage = "请确认您理解此操作的安全影响";
            HasError = true;
            return;
        }

        var deviceInfo = _getDeviceInfo();
        if (string.IsNullOrEmpty(deviceInfo.Password))
        {
            ErrorMessage = "设备密码为空，无法进行密钥部署";
            HasError = true;
            return;
        }

        IsDeploying = true;
        HasError = false;
        ErrorMessage = string.Empty;
        CanRetry = false;
        CanRollback = false;
        ProgressPercent = 0;

        try
        {
            Log.Info($"[SSH] 开始为设备 {deviceInfo.ConnectionName} 部署SSH密钥");

            // 创建进度回调
            var progress = new Progress<(string step, int percent)>(OnProgressUpdated);

            // 执行密钥部署
            var result = await _deploymentService.DeployKeyToDeviceAsync(
                deviceInfo,
                deviceInfo.Password,
                string.IsNullOrEmpty(Passphrase) ? null : Passphrase,
                DisablePasswordAuth,
                progress);

            // 处理结果
            if (result.Success)
            {
                CurrentStep = "部署完成";
                ProgressPercent = 100;
                CanRollback = true;

                Log.Info($"[SSH] 设备 {deviceInfo.ConnectionName} 的SSH密钥部署成功");

                // 显示成功的详细步骤
                foreach (var step in result.Steps.TakeLast(3)) // 显示最后3个重要步骤
                {
                    Log.Info($"[SSH] {step}");
                }
            }
            else
            {
                // 处理失败情况
                if (result.RequiresPassphrase && string.IsNullOrEmpty(Passphrase))
                {
                    CurrentStep = "需要密码短语";
                    ErrorMessage = "私钥需要密码短语，请在上方输入密码短语后重试";
                    HasError = true;
                    CanRetry = true;
                }
                else
                {
                    CurrentStep = "部署失败";
                    ErrorMessage = result.ErrorMessage ?? "未知错误";
                    HasError = true;
                    CanRetry = true;
                }

                Log.Error($"[SSH] 设备 {deviceInfo.ConnectionName} 的SSH密钥部署失败: {result.ErrorMessage}");

                // 记录详细的错误步骤
                foreach (var step in result.Steps)
                {
                    Log.Info($"[SSH] {step}");
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            CanRetry = true;
            CurrentStep = "部署异常";

            Log.Error($"[SSH] 密钥部署过程中发生异常: {ex.Message}", ex);
        }
        finally
        {
            IsDeploying = false;
        }
    }

    /// <summary>
    /// 进度更新回调
    /// </summary>
    /// <param name="update">进度更新信息</param>
    private void OnProgressUpdated((string step, int percent) update)
    {
        CurrentStep = update.step;
        ProgressPercent = update.percent;
    }

    /// <summary>
    /// 回滚操作
    /// </summary>
    private async Task RollbackAsync()
    {
        var deviceInfo = _getDeviceInfo();
        if (string.IsNullOrEmpty(deviceInfo.Password))
        {
            ErrorMessage = "设备密码为空，无法进行回滚操作";
            HasError = true;
            return;
        }

        CurrentStep = "正在回滚操作...";
        ProgressPercent = 0;

        try
        {
            Log.Info($"[SSH] 开始回滚设备 {deviceInfo.ConnectionName} 的SSH密钥部署");

            var result = await _deploymentService.RollbackKeyDeploymentAsync(deviceInfo, deviceInfo.Password);

            if (result.Success)
            {
                CurrentStep = "回滚完成";
                ProgressPercent = 100;
                CanRollback = false;

                Log.Info($"[SSH] 设备 {deviceInfo.ConnectionName} 的SSH密钥回滚成功");
            }
            else
            {
                CurrentStep = "回滚失败";
                ErrorMessage = result.ErrorMessage ?? "回滚操作失败";
                HasError = true;

                Log.Error($"[SSH] 设备 {deviceInfo.ConnectionName} 的SSH密钥回滚失败: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            CurrentStep = "回滚异常";
            ErrorMessage = ex.Message;
            HasError = true;

            Log.Error($"[SSH] 回滚过程中发生异常: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 清除错误状态
    /// </summary>
    private void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
        CanRetry = false;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 更新命令的可执行状态
    /// </summary>
    private void UpdateCommandStates()
    {
        DeployKeyCommand.CanExecute = !IsDeploying && ConfirmOperation;
        RetryDeployCommand.CanExecute = CanRetry;
        RollbackCommand.CanExecute = CanRollback;
        ClearErrorCommand.CanExecute = HasError;
    }

    #endregion
}
