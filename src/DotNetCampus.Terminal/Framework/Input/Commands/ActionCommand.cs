namespace DotNetCampus.Terminal.Framework.Input.Commands;

/// <summary>
/// 表示一个执行动作的命令，此命令不需要传入参数，且没有返回值。
/// </summary>
/// <param name="action">命令执行时，要执行的动作。</param>
public sealed class ActionCommand(Action action) : CanExecuteObservableCommand<ActionCommand>
{
    /// <summary>
    /// 执行命令。
    /// </summary>
    public void Execute()
    {
        try
        {
            action();
            ReportCompleted();
        }
        catch (Exception ex)
        {
            if (!ReportError(ex))
            {
                throw;
            }
        }
    }

    /// <inheritdoc />
    public override void Execute(object? parameter) => Execute();
}

/// <summary>
/// 表示一个执行动作的命令，此命令需要传入一个参数，且没有返回值。
/// </summary>
/// <param name="action">命令执行时，要执行的动作。</param>
/// <typeparam name="T">命令参数的类型。</typeparam>
public sealed class ActionCommand<T>(Action<T> action) : CanExecuteObservableCommand<ActionCommand<T>>
{
    /// <summary>
    /// 执行命令。
    /// </summary>
    /// <param name="parameter">命令参数。</param>
    public void Execute(T parameter)
    {
        try
        {
            action(parameter);
            ReportCompleted();
        }
        catch (Exception ex)
        {
            if (!ReportError(ex))
            {
                throw;
            }
        }
    }

    /// <inheritdoc />
    public override void Execute(object? parameter)
    {
        if (parameter is T t)
        {
            Execute(t);
        }
        else if (parameter is null)
        {
            if (typeof(T).IsValueType)
            {
                throw new ArgumentException($"命令参数期望传入值类型 {typeof(T)}，但实际传入了 null。");
            }

            Execute(default(T)!);
        }
        else
        {
            throw new ArgumentException($"参数类型不匹配，期望类型为 {typeof(T)}，实际类型为 {parameter.GetType()}。");
        }
    }
}
