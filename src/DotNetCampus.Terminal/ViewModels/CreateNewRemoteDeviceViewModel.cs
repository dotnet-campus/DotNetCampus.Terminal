using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Modules.Configurations;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Utils;
using DotNetCampus.Logging;

namespace DotNetCampus.Terminal.ViewModels;

/// <summary>
/// 创建新远程设备的ViewModel
/// </summary>
public record CreateNewRemoteDeviceViewModel : BindableRecord
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigurationManager _configurationManager;
    private readonly MainViewModel _mainViewModel;

    private string _connectionName = string.Empty;
    private string _host = string.Empty;
    private int _port = 22;
    private string _userName = string.Empty;
    private string _password = string.Empty;

    private bool _isTesting = false;
    private bool _isSaving = false;
    private string? _testResultMessage = null;
    private bool? _testResultSuccess = null;

    public CreateNewRemoteDeviceViewModel(IServiceProvider serviceProvider, MainViewModel mainViewModel)
    {
        _serviceProvider = serviceProvider;
        _mainViewModel = mainViewModel;
        _configurationManager = serviceProvider.EnsureGet<ConfigurationManager>();

        TestConnectionCommand = new AsyncCommand(TestConnectionAsync);
        SaveCommand = new AsyncCommand(SaveDeviceAsync);
        CancelCommand = new ActionCommand(Cancel);

        // 初始化命令状态
        UpdateCommandStates();
    }

    #region 属性

    /// <summary>
    /// 连接名称
    /// </summary>
    public string ConnectionName
    {
        get => _connectionName;
        set
        {
            if (SetField(ref _connectionName, value))
            {
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 主机地址
    /// </summary>
    public string Host
    {
        get => _host;
        set
        {
            if (SetField(ref _host, value))
            {
                ClearTestResult();
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 端口号
    /// </summary>
    public int Port
    {
        get => _port;
        set
        {
            if (SetField(ref _port, value))
            {
                ClearTestResult();
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName
    {
        get => _userName;
        set
        {
            if (SetField(ref _userName, value))
            {
                ClearTestResult();
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            if (SetField(ref _password, value))
            {
                ClearTestResult();
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 是否正在测试连接
    /// </summary>
    public bool IsTesting
    {
        get => _isTesting;
        private set
        {
            if (SetField(ref _isTesting, value))
            {
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 是否正在保存
    /// </summary>
    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (SetField(ref _isSaving, value))
            {
                UpdateCommandStates();
            }
        }
    }

    /// <summary>
    /// 测试结果消息
    /// </summary>
    public string? TestResultMessage
    {
        get => _testResultMessage;
        private set => SetField(ref _testResultMessage, value);
    }

    /// <summary>
    /// 测试结果是否成功
    /// </summary>
    public bool? TestResultSuccess
    {
        get => _testResultSuccess;
        private set => SetField(ref _testResultSuccess, value);
    }

    /// <summary>
    /// 是否可以测试连接
    /// </summary>
    public bool CanTestConnection => 
        !string.IsNullOrWhiteSpace(Host) && 
        Port > 0 && Port <= 65535 && 
        !IsTesting && 
        !IsSaving;

    /// <summary>
    /// 是否可以保存
    /// </summary>
    public bool CanSave => 
        !string.IsNullOrWhiteSpace(ConnectionName) &&
        !string.IsNullOrWhiteSpace(Host) &&
        Port > 0 && Port <= 65535 &&
        !string.IsNullOrWhiteSpace(UserName) &&
        TestResultSuccess == true &&
        !IsTesting &&
        !IsSaving;

    #endregion

    #region 命令

    public AsyncCommand TestConnectionCommand { get; }
    public AsyncCommand SaveCommand { get; }
    public ActionCommand CancelCommand { get; }

    #endregion

    #region 私有方法

    /// <summary>
    /// 测试连接
    /// </summary>
    private async Task TestConnectionAsync()
    {
        if (!CanTestConnection)
        {
            return;
        }

        IsTesting = true;
        TestResultSuccess = null;
        TestResultMessage = "正在测试连接...";

        try
        {
            Log.Info($"[UI] 开始测试连接: {UserName}@{Host}:{Port}");

            // 使用NetworkUtils工具类进行TCP连接测试
            var result = await NetworkUtils.TestTcpConnectionAsync(Host, Port);

            if (result)
            {
                TestResultSuccess = true;
                TestResultMessage = $"✓ 连接成功！可以访问 {Host}:{Port}";
                Log.Info($"[UI] 连接测试成功: {Host}:{Port}");
            }
            else
            {
                TestResultSuccess = false;
                TestResultMessage = $"✗ 连接失败，无法访问 {Host}:{Port}";
                Log.Warn($"[UI] 连接测试失败: {Host}:{Port}");
            }
        }
        catch (Exception ex)
        {
            TestResultSuccess = false;
            TestResultMessage = $"✗ 测试连接时发生错误: {ex.Message}";
            Log.Error($"[UI] 测试连接时发生错误: {ex.Message}", ex);
        }
        finally
        {
            IsTesting = false;
            UpdateCommandStates();
        }
    }

    /// <summary>
    /// 保存设备
    /// </summary>
    private async Task SaveDeviceAsync()
    {
        if (!CanSave)
        {
            return;
        }

        IsSaving = true;

        try
        {
            Log.Info($"[UI] 开始保存新设备: {ConnectionName}");

            // 生成本地唯一标识符
            var localId = "device_" + Guid.NewGuid().ToString("N")[..16];

            // 创建设备信息
            var deviceInfo = new SshRemoteDeviceInfo
            {
                LocalId = localId,
                RemoteId = null,
                ConnectionName = ConnectionName,
                Host = Host,
                Port = Port,
                UserName = UserName,
                Password = Password,
                SyncDirectories = [],
            };

            // 保存到配置
            await _configurationManager.SaveRemoteDeviceAsync(deviceInfo);

            Log.Info($"[UI] 新设备保存成功: {ConnectionName}");

            // 重新加载设备列表
            await _mainViewModel.ReloadDevicesCommand.ExecuteAsync();

            // 保存成功后重置表单
            ResetForm();
        }
        catch (Exception ex)
        {
            TestResultSuccess = false;
            TestResultMessage = $"✗ 保存设备失败: {ex.Message}";
            Log.Error($"[UI] 保存设备失败: {ex.Message}", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// 取消操作
    /// </summary>
    private void Cancel()
    {
        ResetForm();
    }

    /// <summary>
    /// 重置表单
    /// </summary>
    private void ResetForm()
    {
        ConnectionName = string.Empty;
        Host = string.Empty;
        Port = 22;
        UserName = string.Empty;
        Password = string.Empty;
        ClearTestResult();
    }

    /// <summary>
    /// 清除测试结果
    /// </summary>
    private void ClearTestResult()
    {
        TestResultSuccess = null;
        TestResultMessage = null;
    }

    /// <summary>
    /// 更新命令状态
    /// </summary>
    private void UpdateCommandStates()
    {
        TestConnectionCommand.CanExecute = CanTestConnection;
        SaveCommand.CanExecute = CanSave;
        OnPropertyChanged(nameof(CanTestConnection));
        OnPropertyChanged(nameof(CanSave));
    }

    #endregion
}

