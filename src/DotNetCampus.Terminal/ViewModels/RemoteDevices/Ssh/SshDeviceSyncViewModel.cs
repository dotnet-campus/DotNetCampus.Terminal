using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Modules.Configurations.Models;
using Avalonia.Collections;

namespace DotNetCampus.Terminal.ViewModels.RemoteDevices.Ssh;

/// <summary>
/// SSH设备同步相关的ViewModel
/// </summary>
public partial record SshDeviceSyncViewModel : TrackableBindableRecord
{
    private readonly IFileSyncService? _fileSyncService;
    private CancellationTokenSource? _syncCancellationTokenSource;
    private double _globalSyncProgress;
    private bool _isGlobalSyncing;
    private DateTimeOffset? _lastSyncTime;
    private string _lastSyncErrorMessage = string.Empty;
    private string _detailedDiagnostics = string.Empty;
    private SyncGroupViewModel? _selectedSyncGroup;

    public SshDeviceSyncViewModel(IFileSyncService? fileSyncService)
    {
        _fileSyncService = fileSyncService;
    }

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
    /// 详细诊断信息
    /// </summary>
    public string DetailedDiagnostics
    {
        get => _detailedDiagnostics;
        private set => SetField(ref _detailedDiagnostics, value);
    }

    /// <summary>
    /// 同步所有启用的目录
    /// </summary>
    public async Task SyncAllAsync(SshRemoteDeviceInfo sshInfo)
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

        var enabledGroups = SyncGroups.Where(sg => sg.IsEnabled).ToList();

        if (enabledGroups.Count == 0)
        {
            Log.Info("[UI] 没有启用的同步组，跳过同步");
            LastSyncErrorMessage = "没有启用的同步组";
            DetailedDiagnostics = "请至少启用一个同步组后再执行同步操作。您可以通过勾选同步组列表中的复选框来启用同步组，或使用「全部启用」按钮一次性启用所有同步组。";
            return;
        }

        // 创建取消令牌
        _syncCancellationTokenSource = new CancellationTokenSource();

        // 设置全局同步状态
        IsGlobalSyncing = true;
        GlobalSyncProgress = 0;
        LastSyncErrorMessage = string.Empty; // 开始同步时清空错误消息
        DetailedDiagnostics = string.Empty; // 清空详细诊断信息

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
            await Task.Delay(100);

            // 根据同步结果更新状态
            switch (result.OverallResult)
            {
                case FileSyncResult.Success:
                    foreach (var group in enabledGroups)
                    {
                        group.Status = SyncGroupStatus.Normal;
                    }
                    LastSyncTime = DateTimeOffset.Now;
                    LastSyncErrorMessage = string.Empty; // 清空错误消息
                    DetailedDiagnostics = string.Empty; // 清空详细诊断信息
                    Log.Info("[UI] 所有目录同步成功");
                    break;
                case FileSyncResult.Failed:
                    foreach (var group in enabledGroups)
                    {
                        group.Status = SyncGroupStatus.Error;
                    }
                    // 使用详细的错误信息
                    LastSyncErrorMessage = result.GetErrorSummary();
                    DetailedDiagnostics = result.GetDetailedDiagnostics();
                    Log.Error($"[UI] 目录同步失败: {LastSyncErrorMessage}");
                    break;
                case FileSyncResult.PartialSuccess:
                    LastSyncTime = DateTimeOffset.Now;
                    // 使用详细的错误信息
                    LastSyncErrorMessage = $"部分同步失败: {result.GetErrorSummary()}";
                    DetailedDiagnostics = result.GetDetailedDiagnostics();
                    Log.Warn($"[UI] 部分目录同步成功，部分失败: {LastSyncErrorMessage}");
                    break;
                case FileSyncResult.Cancelled:
                    foreach (var group in enabledGroups)
                    {
                        group.Status = SyncGroupStatus.Normal;
                    }
                    LastSyncErrorMessage = "同步操作被取消";
                    DetailedDiagnostics = string.Empty;
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
            DetailedDiagnostics = $"异常类型: {ex.GetType().Name}\n错误消息: {ex.Message}\n堆栈跟踪: {ex.StackTrace}";
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
    public async Task CancelSyncAsync()
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
    public async Task EnableAllAsync()
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
    public async Task DisableAllAsync()
    {
        foreach (var group in SyncGroups)
        {
            group.IsEnabled = false;
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// 显示详细诊断信息
    /// </summary>
    public void ShowDiagnostics()
    {
        if (!string.IsNullOrEmpty(DetailedDiagnostics))
        {
            Log.Info($"[UI] 显示详细诊断信息:\n{DetailedDiagnostics}");
            // TODO: 这里可以实现一个对话框或者窗口来显示详细信息
            // 或者将信息复制到剪贴板
        }
        else
        {
            Log.Info("[UI] 没有可用的诊断信息");
        }
    }

    /// <summary>
    /// 初始化同步组数据
    /// </summary>
    public void InitializeSyncGroups(IEnumerable<SyncGroupConfiguration> syncGroupConfigs)
    {
        SyncGroups.Clear();
        
        foreach (var syncGroup in syncGroupConfigs)
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
    /// 获取同步组配置
    /// </summary>
    public List<SyncGroupConfiguration> GetSyncGroupConfigurations()
    {
        return SyncGroups.Select(sg => new SyncGroupConfiguration
        {
            Name = sg.Name,
            RemotePath = sg.RemotePath,
            LocalPath = sg.LocalPath,
            Enabled = sg.Status != SyncGroupStatus.Disabled
        }).ToList();
    }
}
