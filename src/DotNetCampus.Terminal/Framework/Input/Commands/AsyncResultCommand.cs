namespace DotNetCampus.Terminal.Framework.Input.Commands;

/// <summary>
/// 表示一个异步执行动作的命令，此命令不需要传入参数，且有返回值。
/// </summary>
/// <param name="asyncAction">命令执行时，要执行的异步动作。</param>
/// <typeparam name="TResult">命令执行后的返回值类型。</typeparam>
public sealed class AsyncResultCommand<TResult>(Func<Task<TResult>> asyncAction) : CanExecuteObservableCommand<AsyncResultCommand<TResult>, TResult>
{
    /// <summary>
    /// 执行命令并异步等待命令执行结果。
    /// </summary>
    /// <returns>命令执行后的结果。</returns>
    public async Task<TResult> ExecuteAsync()
    {
        try
        {
            var result = await asyncAction();
            ReportCompleted(result);
            return result;
        }
        catch (Exception ex)
        {
            if (!ReportError(ex))
            {
                throw;
            }

            return default!;
        }
    }

    /// <inheritdoc />
    public override async void Execute(object? parameter)
    {
        await ExecuteAsync();
    }
}

/// <summary>
/// 表示一个异步执行动作的命令，此命令需要传入一个参数，且有返回值。
/// </summary>
/// <param name="asyncAction">命令执行时，要执行的异步动作。</param>
/// <typeparam name="T">命令参数的类型。</typeparam>
/// <typeparam name="TResult">命令执行后的返回值类型。</typeparam>
public sealed class AsyncResultCommand<T, TResult>(Func<T, Task<TResult>> asyncAction) : CanExecuteObservableCommand<AsyncResultCommand<T, TResult>, TResult>
{
    /// <summary>
    /// 执行命令并异步等待命令执行结果。
    /// </summary>
    /// <param name="parameter">命令参数。</param>
    /// <returns>命令执行后的结果。</returns>
    public async Task<TResult> ExecuteAsync(T parameter)
    {
        try
        {
            var result = await asyncAction(parameter);
            ReportCompleted(result);
            return result;
        }
        catch (Exception ex)
        {
            if (!ReportError(ex))
            {
                throw;
            }

            return default!;
        }
    }

    /// <inheritdoc />
    public override async void Execute(object? parameter)
    {
        if (parameter is T t)
        {
            await ExecuteAsync(t);
        }
        else if (parameter is null)
        {
            if (typeof(T).IsValueType)
            {
                throw new ArgumentException($"命令参数期望传入值类型 {typeof(T)}，但实际传入了 null。");
            }

            await ExecuteAsync(default!);
        }
        else
        {
            throw new ArgumentException($"参数类型不匹配，期望类型为 {typeof(T)}，实际类型为 {parameter.GetType()}。");
        }
    }
}
