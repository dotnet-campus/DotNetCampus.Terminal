namespace DotNetCampus.Terminal.Framework.Input.Commands;

/// <summary>
/// 手动控制是否可以执行的命令，但可以在命令执行完成或发生错误时执行回调。
/// </summary>
/// <typeparam name="TCommand">命令自身的具体类型。</typeparam>
public abstract class CanExecuteObservableCommand<TCommand> : CanExecuteCommand
    where TCommand : CanExecuteObservableCommand<TCommand>
{
    private Action? _onCompleted;
    private Action<Exception>? _onError;

    /// <summary>
    /// UI 层调用此方法以在命令成功执行完成时执行某些交互操作。
    /// </summary>
    /// <param name="action">成功执行完成时要执行的操作。</param>
    /// <returns>构造器模式。</returns>
    public TCommand WhenSuccessfullyCompleted(Action action)
    {
        _onCompleted = action;
        return (TCommand)this;
    }

    /// <summary>
    /// UI 层调用此方法以在命令执行过程中发生错误时执行某些交互操作。
    /// </summary>
    /// <param name="action">发生错误时要执行的操作。</param>
    /// <returns>构造器模式。</returns>
    /// <remarks>
    /// 注意，如果不调用此方法而命令执行发生未捕获的异常时，异常会被 <see cref="TaskScheduler.UnobservedTaskException"/> 捕获。
    /// </remarks>
    public TCommand WhenErrorOccurred(Action<Exception> action)
    {
        _onError = action;
        return (TCommand)this;
    }

    /// <summary>
    /// 具体的派生命令实现时，在命令成功执行完成时调用此方法。
    /// </summary>
    protected void ReportCompleted()
    {
        _onCompleted?.Invoke();
    }

    /// <summary>
    /// 具体的派生命令实现时，在命令执行过程中发生错误时调用此方法。
    /// </summary>
    /// <param name="exception">发生的异常。</param>
    /// <returns>如果异常已被处理，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    protected bool ReportError(Exception exception)
    {
        if (_onError is null)
        {
            return false;
        }

        _onError.Invoke(exception);
        return true;
    }
}

/// <summary>
/// 手动控制是否可以执行的命令，但可以在命令执行完成或发生错误时执行回调。此命令有返回值。
/// </summary>
/// <typeparam name="TCommand">命令自身的具体类型。</typeparam>
/// <typeparam name="TResult">命令执行完成后的返回值类型。</typeparam>
public abstract class CanExecuteObservableCommand<TCommand, TResult> : CanExecuteCommand
    where TCommand : CanExecuteObservableCommand<TCommand, TResult>
{
    private Action<TResult>? _onCompleted;
    private Action<Exception>? _onError;

    /// <summary>
    /// UI 层调用此方法以在命令成功执行完成时执行某些交互操作。
    /// </summary>
    /// <param name="action">成功执行完成时要执行的操作。参数为命令执行完成后的返回值。</param>
    /// <returns>构造器模式。</returns>
    public TCommand WhenSuccessfullyCompleted(Action<TResult> action)
    {
        _onCompleted = action;
        return (TCommand)this;
    }

    /// <summary>
    /// UI 层调用此方法以在命令执行过程中发生错误时执行某些交互操作。
    /// </summary>
    /// <param name="action">发生错误时要执行的操作。</param>
    /// <returns>构造器模式。</returns>
    public TCommand WhenErrorOccurred(Action<Exception> action)
    {
        _onError = action;
        return (TCommand)this;
    }

    /// <summary>
    /// 具体的派生命令实现时，在命令成功执行完成时调用此方法，并传入返回值。
    /// </summary>
    /// <param name="result">命令执行完成后的返回值。</param>
    protected void ReportCompleted(TResult result)
    {
        _onCompleted?.Invoke(result);
    }

    /// <summary>
    /// 具体的派生命令实现时，在命令执行过程中发生错误时调用此方法。
    /// </summary>
    /// <param name="exception">发生的异常。</param>
    /// <returns>如果异常已被处理，则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    protected bool ReportError(Exception exception)
    {
        if (_onError is null)
        {
            return false;
        }

        _onError.Invoke(exception);
        return true;
    }
}
