using System.Runtime.CompilerServices;

namespace DotNetCampus.Terminal.Framework.Mvvm;

/// <summary>
/// 提供「未保存」状态的追踪，属性绑定时会自动更新未保存状态。
/// </summary>
public record TrackingUnsavedBindableRecord : BindableRecord
{
    private bool _hasUnsavedChanges;

    /// <summary>
    /// 获取或设置是否有未保存的变更。
    /// </summary>
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set => SetField(ref _hasUnsavedChanges, value);
    }

    /// <summary>
    /// 重置未保存变更状态，标记为已保存无变更。
    /// </summary>
    public virtual void MarkAsSaved()
    {
        HasUnsavedChanges = false;
    }

    /// <summary>
    /// 手动标记有未保存的变更。
    /// </summary>
    public void MarkAsUnsaved()
    {
        HasUnsavedChanges = true;
    }

    /// <summary>
    /// 设置字段值并跟踪变更，如果值发生变化则标记为有未保存的变更
    /// </summary>
    /// <typeparam name="T">字段类型</typeparam>
    /// <param name="field">字段引用</param>
    /// <param name="value">新值</param>
    /// <param name="propertyName">属性名称，自动推断</param>
    /// <returns>如果值已更改则返回 true，否则返回 false</returns>
    protected bool SetFieldAndUnsaved<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        var changed = SetField(ref field, value, propertyName);
        if (changed)
        {
            HasUnsavedChanges = true;
        }
        return changed;
    }
}
