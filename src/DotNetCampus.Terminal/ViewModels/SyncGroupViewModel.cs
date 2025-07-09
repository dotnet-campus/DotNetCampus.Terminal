using DotNetCampus.Terminal.FileSync;
using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.ViewModels;

/// <summary>
/// 同步组视图模型
/// </summary>
public record SyncGroupViewModel : BindableRecord
{
    private string _name = string.Empty;
    private string _remotePath = string.Empty;
    private string _localPath = string.Empty;
    private SyncGroupStatus _status = SyncGroupStatus.Normal;
    private string _statusText = string.Empty;
    private double _syncProgress;
    private bool _isEnabled = true;
    private bool _isSyncing;
    private SyncDirection _direction = SyncDirection.LocalToRemote;

    public SyncGroupViewModel()
    {
        SyncCommand = new AsyncCommand(OnSyncAsync);
        UpdateStatusDisplay();
        UpdateDirectionDisplay();
    }

    /// <summary>
    /// 同步命令
    /// </summary>
    public AsyncCommand SyncCommand { get; }

    /// <summary>
    /// 同步组名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// 远程路径
    /// </summary>
    public string RemotePath
    {
        get => _remotePath;
        set => SetField(ref _remotePath, value);
    }

    /// <summary>
    /// 本地路径
    /// </summary>
    public string LocalPath
    {
        get => _localPath;
        set => SetField(ref _localPath, value);
    }

    /// <summary>
    /// 同步状态
    /// </summary>
    public SyncGroupStatus Status
    {
        get => _status;
        set
        {
            if (SetField(ref _status, value))
            {
                UpdateStatusDisplay();
                OnPropertyChanged(nameof(StatusSymbol));
                OnPropertyChanged(nameof(StatusColor));
            }
        }
    }

    /// <summary>
    /// 状态文本
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (SetField(ref _isEnabled, value))
            {
                Status = value ? SyncGroupStatus.Normal : SyncGroupStatus.Disabled;
            }
        }
    }

    /// <summary>
    /// 是否正在同步
    /// </summary>
    public bool IsSyncing
    {
        get => _isSyncing;
        private set => SetField(ref _isSyncing, value);
    }

    /// <summary>
    /// 同步进度 (0-100)
    /// </summary>
    public double SyncProgress
    {
        get => _syncProgress;
        set => SetField(ref _syncProgress, value);
    }

    /// <summary>
    /// 状态符号
    /// </summary>
    public string StatusSymbol => Status switch
    {
        SyncGroupStatus.Normal => "✓",
        SyncGroupStatus.Error => "⚠",
        SyncGroupStatus.Disabled => "✗",
        SyncGroupStatus.Syncing => "◐",
        _ => "○"
    };

    /// <summary>
    /// 状态颜色
    /// </summary>
    public string StatusColor => Status switch
    {
        SyncGroupStatus.Normal => "Green",
        SyncGroupStatus.Error => "Yellow",
        SyncGroupStatus.Disabled => "Red",
        SyncGroupStatus.Syncing => "Cyan",
        _ => "DimGray"
    };

    private void UpdateStatusDisplay()
    {
        StatusText = Status switch
        {
            SyncGroupStatus.Normal => string.Empty,
            SyncGroupStatus.Error => "(同步出错)",
            SyncGroupStatus.Disabled => "(已禁用)",
            SyncGroupStatus.Syncing => "(同步中)",
            _ => string.Empty
        };

        IsSyncing = Status == SyncGroupStatus.Syncing;
    }

    /// <summary>
    /// 执行同步操作
    /// </summary>
    private async Task OnSyncAsync()
    {
        if (Status == SyncGroupStatus.Syncing)
        {
            return;
        }

        Status = SyncGroupStatus.Syncing;
        SyncProgress = 0;

        try
        {
            // 这里需要获取父ViewModel中的SshInfo和FileSyncService
            // 当前直接通过UpdateStatus模拟进度展示

            for (int i = 0; i <= 10; i++)
            {
                SyncProgress = i * 10;
                await Task.Delay(200); // 模拟同步过程
            }

            Status = SyncGroupStatus.Normal;
            SyncProgress = 100;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"同步过程中发生错误: {ex.Message}");
            Status = SyncGroupStatus.Error;
        }
    }

    /// <summary>
    /// 同步方向
    /// </summary>
    public SyncDirection Direction
    {
        get => _direction;
        set
        {
            if (SetField(ref _direction, value))
            {
                UpdateDirectionDisplay();
                OnPropertyChanged(nameof(DirectionText));
                OnPropertyChanged(nameof(DirectionSymbol));
            }
        }
    }

    /// <summary>
    /// 同步方向显示文本
    /// </summary>
    public string DirectionText { get; private set; } = string.Empty;

    /// <summary>
    /// 同步方向符号
    /// </summary>
    public string DirectionSymbol { get; private set; } = "↑";

    private void UpdateDirectionDisplay()
    {
        DirectionText = SyncDirectionParser.ToDisplayText(Direction);

        DirectionSymbol = Direction switch
        {
            SyncDirection.LocalToRemote => "↑",
            SyncDirection.RemoteToLocal => "↓",
            _ => "?"
        };
    }
}

/// <summary>
/// 同步组状态
/// </summary>
public enum SyncGroupStatus
{
    /// <summary>
    /// 正常
    /// </summary>
    Normal,

    /// <summary>
    /// 出错
    /// </summary>
    Error,

    /// <summary>
    /// 禁用
    /// </summary>
    Disabled,

    /// <summary>
    /// 同步中
    /// </summary>
    Syncing
}
