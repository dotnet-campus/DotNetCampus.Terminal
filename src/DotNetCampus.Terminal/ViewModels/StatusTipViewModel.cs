using DotNetCampus.Terminal.Framework.Mvvm;

namespace DotNetCampus.Terminal.ViewModels;

/// <summary>
/// 状态提示ViewModel，用于全局状态显示和功能提示
/// </summary>
public record StatusTipViewModel : BindableRecord
{
    private string _currentTip = "就绪 - 使用F1-F10功能键操作";
    private bool _isError = false;
    private bool _isWarning = false;

    /// <summary>
    /// 当前提示信息
    /// </summary>
    public string CurrentTip
    {
        get => _currentTip;
        set => SetField(ref _currentTip, value);
    }

    /// <summary>
    /// 是否为错误状态
    /// </summary>
    public bool IsError
    {
        get => _isError;
        set => SetField(ref _isError, value);
    }

    /// <summary>
    /// 是否为警告状态
    /// </summary>
    public bool IsWarning
    {
        get => _isWarning;
        set => SetField(ref _isWarning, value);
    }

    /// <summary>
    /// 是否为正常状态（既不是错误也不是警告）
    /// </summary>
    public bool IsNormal => !IsError && !IsWarning;

    /// <summary>
    /// 显示普通提示
    /// </summary>
    public void ShowTip(string message)
    {
        CurrentTip = message;
        IsError = false;
        IsWarning = false;
    }

    /// <summary>
    /// 显示错误提示
    /// </summary>
    public void ShowError(string message)
    {
        CurrentTip = $"错误: {message}";
        IsError = true;
        IsWarning = false;
    }

    /// <summary>
    /// 显示警告提示
    /// </summary>
    public void ShowWarning(string message)
    {
        CurrentTip = $"警告: {message}";
        IsError = false;
        IsWarning = true;
    }

    /// <summary>
    /// 显示功能键提示
    /// </summary>
    public void ShowFunctionKeyTip(string functionKey, string description, bool isEnabled = true)
    {
        if (isEnabled)
        {
            CurrentTip = $"{functionKey} - {description}";
        }
        else
        {
            CurrentTip = $"{functionKey} - {description} (需要选中设备)";
            IsWarning = true;
        }
        IsError = false;
    }

    /// <summary>
    /// 显示操作状态
    /// </summary>
    public void ShowOperationStatus(string operation, bool isInProgress = false)
    {
        if (isInProgress)
        {
            CurrentTip = $"正在{operation}...";
        }
        else
        {
            CurrentTip = $"{operation}完成";
        }
        IsError = false;
        IsWarning = false;
    }

    /// <summary>
    /// 重置为默认状态
    /// </summary>
    public void Reset()
    {
        CurrentTip = "就绪 - 使用F1-F10功能键操作";
        IsError = false;
        IsWarning = false;
    }
}
