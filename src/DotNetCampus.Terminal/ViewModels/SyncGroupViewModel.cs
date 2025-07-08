using DotNetCampus.Terminal.Framework.Mvvm;

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
