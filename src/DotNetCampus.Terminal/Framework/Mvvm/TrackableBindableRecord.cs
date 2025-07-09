using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DotNetCampus.Terminal.Framework.Mvvm;

/// <summary>
/// 可跟踪变更的可绑定记录基类，提供未保存变更状态跟踪
/// </summary>
public record TrackableBindableRecord : BindableRecord
{
    private bool _hasUnsavedChanges;

    /// <summary>
    /// 获取或设置是否有未保存的变更
    /// </summary>
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => SetField(ref _hasUnsavedChanges, value);
    }

    /// <summary>
    /// 重置未保存变更状态
    /// </summary>
    public virtual void ResetChangeTracking()
    {
        HasUnsavedChanges = false;
    }

    /// <summary>
    /// 设置字段值并跟踪变更，如果值发生变化则标记为有未保存的变更
    /// </summary>
    /// <typeparam name="T">字段类型</typeparam>
    /// <param name="field">字段引用</param>
    /// <param name="value">新值</param>
    /// <param name="propertyName">属性名称，自动推断</param>
    /// <returns>如果值已更改则返回 true，否则返回 false</returns>
    protected bool SetFieldTrackingChanges<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        bool changed = SetField(ref field, value, propertyName);
        if (changed)
        {
            HasUnsavedChanges = true;
        }
        return changed;
    }
    
    /// <summary>
    /// 手动标记有未保存的变更
    /// </summary>
    protected void MarkAsChanged()
    {
        HasUnsavedChanges = true;
    }
}
