using DotNetCampus.Terminal.Framework.Input.Commands;
using DotNetCampus.Terminal.Framework.Mvvm;
using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.ViewModels;

/// <summary>
/// 本地与远程设备同步一组目录的视图模型。
/// </summary>
public record DirectorySyncingViewModel : TrackingUnsavedBindableRecord
{
    private string _name = string.Empty;
    private string _remotePath = string.Empty;
    private string _localPath = string.Empty;
    private DirectorySyncingStatus _status = DirectorySyncingStatus.Normal;
    private string _statusText = string.Empty;
    private double _syncProgress;
    private bool _isEnabled = true;
    private bool _isSyncing;
    private SyncDirection _direction = SyncDirection.LocalToRemote;

    public DirectorySyncingViewModel()
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
        set => SetFieldAndUnsaved(ref _name, value);
    }

    /// <summary>
    /// 远程路径
    /// </summary>
    public string RemotePath
    {
        get => _remotePath;
        set => SetFieldAndUnsaved(ref _remotePath, value);
    }

    /// <summary>
    /// 本地路径
    /// </summary>
    public string LocalPath
    {
        get => _localPath;
        set => SetFieldAndUnsaved(ref _localPath, value);
    }

    /// <summary>
    /// 同步状态
    /// </summary>
    public DirectorySyncingStatus Status
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
            if (SetFieldAndUnsaved(ref _isEnabled, value))
            {
                Status = value ? DirectorySyncingStatus.Normal : DirectorySyncingStatus.Disabled;
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
    /// 状态符号
    /// </summary>
    public string StatusSymbol => Status switch
    {
        DirectorySyncingStatus.Normal => "✓",
        DirectorySyncingStatus.Error => "⚠",
        DirectorySyncingStatus.Disabled => "✗",
        DirectorySyncingStatus.Syncing => "◐",
        _ => "○",
    };

    /// <summary>
    /// 状态颜色
    /// </summary>
    public string StatusColor => Status switch
    {
        DirectorySyncingStatus.Normal => "Green",
        DirectorySyncingStatus.Error => "Yellow",
        DirectorySyncingStatus.Disabled => "Red",
        DirectorySyncingStatus.Syncing => "Cyan",
        _ => "DimGray",
    };

    private void UpdateStatusDisplay()
    {
        StatusText = Status switch
        {
            DirectorySyncingStatus.Normal => string.Empty,
            DirectorySyncingStatus.Error => "(同步出错)",
            DirectorySyncingStatus.Disabled => "(已禁用)",
            DirectorySyncingStatus.Syncing => "(同步中)",
            _ => string.Empty,
        };

        IsSyncing = Status == DirectorySyncingStatus.Syncing;
    }

    /// <summary>
    /// 执行同步操作
    /// </summary>
    private async Task OnSyncAsync()
    {
        if (Status == DirectorySyncingStatus.Syncing)
        {
            return;
        }

        Status = DirectorySyncingStatus.Syncing;
        SyncProgress = 0;

        try
        {
            // 这里需要获取父ViewModel中的SshInfo和FileSyncService
            // 当前直接通过UpdateStatus模拟进度展示

            for (var i = 0; i <= 10; i++)
            {
                SyncProgress = i * 10;
                await Task.Delay(200); // 模拟同步过程
            }

            Status = DirectorySyncingStatus.Normal;
            SyncProgress = 100;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"同步过程中发生错误: {ex.Message}");
            Status = DirectorySyncingStatus.Error;
        }
    }

    /// <summary>
    /// 同步方向显示文本
    /// </summary>
    public string DirectionText { get; private set; } = string.Empty;

    /// <summary>
    /// 同步方向符号
    /// </summary>
    public string DirectionSymbol { get; private set; } = "↑ 上传";

    private void UpdateDirectionDisplay()
    {
        DirectionText = SyncDirectionParser.ToDisplayText(Direction);

        DirectionSymbol = Direction switch
        {
            SyncDirection.LocalToRemote => "↑ 上传",
            SyncDirection.RemoteToLocal => "↓ 下载",
            _ => "?",
        };
    }
}

/// <summary>
/// 目录同步状态
/// </summary>
public enum DirectorySyncingStatus
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
    Syncing,
}
