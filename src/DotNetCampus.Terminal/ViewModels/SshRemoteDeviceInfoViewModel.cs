using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync;
using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using DotNetCampus.Terminal.Utils;
using Avalonia.Collections;
using DotNetCampus.Terminal.Framework.DependencyInjection;

namespace DotNetCampus.Terminal.ViewModels;

public record SshRemoteDeviceInfoViewModel : RemoteDeviceInfoNode
{
    private string _connectionName;
    private string _host;
    private int _port;
    private string _userName;
    private string? _password;
    private SyncGroupViewModel? _selectedSyncGroup;
    private readonly IFileSyncService? _fileSyncService;
    private CancellationTokenSource? _syncCancellationTokenSource;
    private double _globalSyncProgress;
    private bool _isGlobalSyncing;
    private DateTimeOffset? _lastSyncTime;
    private string _lastSyncErrorMessage = string.Empty;

    public SshRemoteDeviceInfoViewModel() : base(new SshRemoteDeviceInfo
    {
        ConnectionName = "设计时设备",
        Host = "localhost",
        Port = 22,
        UserName = "username",
        Password = "password",
        SyncGroups = [
            new SyncGroupConfiguration
            {
                Name = "设计时项目",
                RemotePath = "/home/user/projects/design",
                LocalPath = @"D:\Projects\Design",
                Enabled = true
            },
            new SyncGroupConfiguration
            {
                Name = "设计时文档",
                RemotePath = "/home/user/documents",
                LocalPath = @"D:\Documents",
                Enabled = false
            }
        ]
    })
    {
        _connectionName = "设计时设备";
        _host = "localhost";
        _port = 22;
        _userName = "username";
        _password = "password";

        // 初始化命令
        InitializeCommands();

        // 添加设计时示例数据
        SyncGroups.Add(new SyncGroupViewModel
        {
            Name = "设计时项目",
            RemotePath = "/home/user/projects/design",
            LocalPath = @"D:\Projects\Design",
            Status = SyncGroupStatus.Normal,
            Direction = SyncDirection.LocalToRemote
        });

        SyncGroups.Add(new SyncGroupViewModel
        {
            Name = "设计时文档",
            RemotePath = "/home/user/documents",
            LocalPath = @"D:\Documents",
            Status = SyncGroupStatus.Disabled,
            Direction = SyncDirection.RemoteToLocal
        });

        SyncGroups.Add(new SyncGroupViewModel
        {
            Name = "配置备份",
            RemotePath = "/home/user/backup",
            LocalPath = @"D:\Backup",
            Status = SyncGroupStatus.Normal,
            Direction = SyncDirection.RemoteToLocal
        });
    }

    public SshRemoteDeviceInfoViewModel(SshRemoteDeviceInfo info) : base(info)
    {
        _connectionName = info.ConnectionName;
        _host = info.Host;
        _port = info.Port;
        _userName = info.UserName;
        _password = info.Password;
        _fileSyncService = Container.Current.EnsureGet<IFileSyncService>();

        // 初始化命令
        InitializeCommands();

        // 从配置中加载同步组数据
        foreach (var syncGroup in info.SyncGroups)
        {
            SyncGroups.Add(new SyncGroupViewModel
            {
                Name = syncGroup.Name,
                RemotePath = syncGroup.RemotePath,
                LocalPath = syncGroup.LocalPath,
                IsEnabled = syncGroup.Enabled,
                Direction = syncGroup.DirectionEnum
            });
        }

        // 如果没有配置同步组，添加一个示例（仅用于演示）
        if (SyncGroups.Count == 0)
        {
            SyncGroups.Add(new SyncGroupViewModel
            {
                Name = "示例同步组",
                RemotePath = "/home/user/example",
                LocalPath = @"D:\Example",
                Status = SyncGroupStatus.Normal
            });
        }
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
    /// 同步组列表
    /// </summary>
    public AvaloniaList<SyncGroupViewModel> SyncGroups { get; } = new();

    /// <summary>
    /// 选中的同步组
    /// </summary>
    public SyncGroupViewModel? SelectedSyncGroup
    {
        get => _selectedSyncGroup;
        set => SetField(ref _selectedSyncGroup, value);
    }

    public string ConnectionName
    {
        get => _connectionName;
        set => SetField(ref _connectionName, value);
    }

    public string Host
    {
        get => _host;
        set
        {
            if (SetField(ref _host, value))
            {
                OnPropertyChanged(nameof(Address));
            }
        }
    }

    public int Port
    {
        get => _port;
        set
        {
            if (SetField(ref _port, value))
            {
                OnPropertyChanged(nameof(Address));
            }
        }
    }

    public string Address => Port is 22
        ? Host
        : $"{Host}:{Port}";

    public string UserName
    {
        get => _userName;
        set => SetField(ref _userName, value);
    }

    public string? Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    /// <summary>
    /// 全局同步进度 (0-100)
    /// </summary>
    public double GlobalSyncProgress
    {
        get => _globalSyncProgress;
        private set => SetField(ref _globalSyncProgress, value);
    }

    /// <summary>
    /// 是否正在进行全局同步
    /// </summary>
    public bool IsGlobalSyncing
    {
        get => _isGlobalSyncing;
        private set => SetField(ref _isGlobalSyncing, value);
    }

    /// <summary>
    /// 最近同步时间
    /// </summary>
    public DateTimeOffset? LastSyncTime
    {
        get => _lastSyncTime;
        private set => SetField(ref _lastSyncTime, value);
    }

    /// <summary>
    /// 最近同步错误消息
    /// </summary>
    public string LastSyncErrorMessage
    {
        get => _lastSyncErrorMessage;
        private set => SetField(ref _lastSyncErrorMessage, value);
    }

    /// <summary>
    /// 初始化命令
    /// </summary>
    private void InitializeCommands()
    {
        SyncAllCommand = new AsyncCommand(OnSyncAllAsync);
        CancelSyncCommand = new AsyncCommand(OnCancelSyncAsync);
        EnableAllCommand = new AsyncCommand(OnEnableAllAsync);
        DisableAllCommand = new AsyncCommand(OnDisableAllAsync);
    }

    protected override async Task<bool> OnTestConnectionAsync()
    {
        var sshInfo = (SshRemoteDeviceInfo)Info;

        // 使用NetworkUtils工具类进行TCP连接测试
        return await NetworkUtils.TestTcpConnectionAsync(sshInfo.Host, sshInfo.Port);
    }

    /// <summary>
    /// 同步所有启用的目录
    /// </summary>
    private async Task OnSyncAllAsync()
    {
        // 如果文件同步服务未注入，则直接返回
        if (_fileSyncService == null)
        {
            Log.Error("[UI] 文件同步服务未初始化，无法执行同步操作");
            return;
        }

        // 如果已经有同步任务在进行中，则不执行新的同步
        if (_syncCancellationTokenSource != null)
        {
            Log.Warn("[UI] 已有同步任务正在进行中");
            return;
        }

        var sshInfo = (SshRemoteDeviceInfo)Info;
        var enabledGroups = SyncGroups.Where(sg => sg.IsEnabled).ToList();

        if (enabledGroups.Count == 0)
        {
            Log.Info("[UI] 没有启用的同步组，跳过同步");
            return;
        }

        // 创建取消令牌
        _syncCancellationTokenSource = new CancellationTokenSource();

        // 设置全局同步状态
        IsGlobalSyncing = true;
        GlobalSyncProgress = 0;
        LastSyncErrorMessage = string.Empty; // 开始同步时清空错误消息

        // 将所有启用的同步组状态设置为同步中
        foreach (var group in enabledGroups)
        {
            group.Status = SyncGroupStatus.Syncing;
        }

        try
        {
            // 构建同步配置
            var syncConfigs = enabledGroups.Select(group => new SyncGroupConfiguration
            {
                Name = group.Name,
                RemotePath = group.RemotePath,
                LocalPath = group.LocalPath,
                Enabled = true,
                Direction = group.Direction.ToString() // 设置字符串值，会通过解析器转换为枚举
            }).ToList();

            // 创建进度报告
            var progress = new Progress<FileSyncProgress>(p =>
            {
                // 更新全局进度
                GlobalSyncProgress = p.TotalProgress;

                // 查找当前处理的文件所属的同步组
                var currentGroup = enabledGroups.FirstOrDefault(
                    g => p.CurrentFile.StartsWith(g.LocalPath, StringComparison.OrdinalIgnoreCase));

                if (currentGroup != null)
                {
                    currentGroup.SyncProgress = p.CurrentFileProgress;
                }

                Log.Debug($"[UI] 同步进度: {p.TotalProgress:F2}%, 当前文件: {p.CurrentFile}");
            });

            // 执行同步
            var result = await _fileSyncService.SyncMultipleDirectoriesAsync(
                sshInfo, syncConfigs, progress, _syncCancellationTokenSource.Token);

            // 根据同步结果更新状态
            switch (result)
            {
                case FileSyncResult.Success:
                    foreach (var group in enabledGroups)
                    {
                        group.Status = SyncGroupStatus.Normal;
                    }
                    LastSyncTime = DateTimeOffset.Now;
                    LastSyncErrorMessage = string.Empty; // 清空错误消息
                    Log.Info("[UI] 所有目录同步成功");
                    break;
                case FileSyncResult.Failed:
                    foreach (var group in enabledGroups)
                    {
                        group.Status = SyncGroupStatus.Error;
                    }
                    LastSyncErrorMessage = "同步失败";
                    Log.Error("[UI] 目录同步失败");
                    break;
                case FileSyncResult.PartialSuccess:
                    LastSyncTime = DateTimeOffset.Now;
                    LastSyncErrorMessage = "部分目录同步失败";
                    Log.Warn("[UI] 部分目录同步成功，部分失败");
                    break;
                case FileSyncResult.Cancelled:
                    foreach (var group in enabledGroups)
                    {
                        group.Status = SyncGroupStatus.Normal;
                    }
                    LastSyncErrorMessage = "同步操作被取消";
                    Log.Info("[UI] 同步操作被取消");
                    break;
            }
        }
        catch (Exception ex)
        {
            // 发生异常时将所有同步组状态设置为错误
            foreach (var group in enabledGroups)
            {
                group.Status = SyncGroupStatus.Error;
            }
            LastSyncErrorMessage = $"同步异常: {ex.Message}";
            Log.Error($"[UI] 同步过程中发生错误: {ex.Message}");
        }
        finally
        {
            // 清理取消令牌
            _syncCancellationTokenSource.Dispose();
            _syncCancellationTokenSource = null;
            
            // 重置全局同步状态
            IsGlobalSyncing = false;
            GlobalSyncProgress = 0;
        }
    }

    /// <summary>
    /// 取消正在进行的同步
    /// </summary>
    private async Task OnCancelSyncAsync()
    {
        if (_syncCancellationTokenSource == null)
        {
            return;
        }

        try
        {
            _syncCancellationTokenSource.Cancel();
            Log.Info("[UI] 已发送取消同步请求");

            // 等待取消完成
            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            Log.Error($"[UI] 取消同步时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 启用所有同步组
    /// </summary>
    private async Task OnEnableAllAsync()
    {
        foreach (var group in SyncGroups)
        {
            group.IsEnabled = true;
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// 禁用所有同步组
    /// </summary>
    private async Task OnDisableAllAsync()
    {
        foreach (var group in SyncGroups)
        {
            group.IsEnabled = false;
        }
        await Task.CompletedTask;
    }
}
