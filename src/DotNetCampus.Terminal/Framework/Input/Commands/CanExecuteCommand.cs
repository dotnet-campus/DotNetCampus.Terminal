using System.Windows.Input;

namespace DotNetCampus.Terminal.Framework.Input.Commands;

/// <summary>
/// 手动控制是否可以执行的命令。
/// </summary>
public abstract class CanExecuteCommand : ICommand
{
    private bool _canExecute = true;
    private event EventHandler? CanExecuteChanged;

    /// <summary>
    /// 通过设置此属性来控制是否可以执行。
    /// </summary>
    public bool CanExecute
    {
        get => _canExecute;
        set
        {
            if (_canExecute == value)
            {
                return;
            }

            _canExecute = value;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    event EventHandler? ICommand.CanExecuteChanged
    {
        add => this.CanExecuteChanged += value;
        remove => this.CanExecuteChanged -= value;
    }

    bool ICommand.CanExecute(object? parameter) => CanExecute;

    /// <summary>
    /// 命令的执行方法。
    /// </summary>
    /// <param name="parameter">命令参数。</param>
    public abstract void Execute(object? parameter);
}
