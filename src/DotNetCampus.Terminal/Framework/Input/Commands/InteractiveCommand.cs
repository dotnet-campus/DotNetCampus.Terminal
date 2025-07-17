namespace DotNetCampus.Terminal.Framework.Input.Commands;

/// <summary>
/// 表示一个交互式命令，命令执行过程中可能会一次或多次要求 UI 响应交互。此命令不需要传入参数。
/// </summary>
/// <param name="action">命令执行时，要执行的动作。</param>
/// <typeparam name="TInteraction">交互式命令的交互细节，请自行定义一个记录（record）继承自 <see cref="InteractiveCommandInteraction"/>。</typeparam>
public class InteractiveCommand<TInteraction>(Action<TInteraction> action)
    : CanExecuteObservableCommand<InteractiveCommand<TInteraction>>
    where TInteraction : InteractiveCommandInteraction
{
    private TInteraction? _interaction;

    /// <summary>
    /// 交互式执行命令。
    /// </summary>
    /// <exception cref="InvalidOperationException">如果执行命令前没有初始化交互，则抛出此异常。</exception>
    public void Execute()
    {
        if (_interaction is null)
        {
            throw new InvalidOperationException("The interaction must be provided before executing the command.");
        }

        try
        {
            action(_interaction!);
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

    /// <summary>
    /// UI 层请调用此方法以提供用户交互的具体实现。
    /// </summary>
    /// <param name="interaction">交互式命令的交互细节，请自行定义一个记录（record）继承自 <see cref="InteractiveCommandInteraction"/>。</param>
    /// <returns>构造器模式。</returns>
    public InteractiveCommand<TInteraction> ProvideInteraction(TInteraction interaction)
    {
        _interaction = interaction;
        return this;
    }
}

/// <summary>
/// 表示一个交互式命令，命令执行过程中可能会一次或多次要求 UI 响应交互。此命令需要传入一个参数。
/// </summary>
/// <param name="action">命令执行时，要执行的动作。</param>
/// <typeparam name="TInteraction">交互式命令的交互细节，请自行定义一个记录（record）继承自 <see cref="InteractiveCommandInteraction"/>。</typeparam>
/// <typeparam name="TParameter">命令参数的类型。</typeparam>
public class InteractiveCommand<TInteraction, TParameter>(Action<TInteraction, TParameter> action)
    : CanExecuteObservableCommand<InteractiveCommand<TInteraction>>
    where TInteraction : InteractiveCommandInteraction
{
    private TInteraction? _interaction;

    /// <summary>
    /// 交互式执行命令。
    /// </summary>
    /// <param name="parameter">命令参数。</param>
    /// <exception cref="InvalidOperationException">如果执行命令前没有初始化交互，则抛出此异常。</exception>
    public void Execute(TParameter parameter)
    {
        if (_interaction is null)
        {
            throw new InvalidOperationException("The interaction must be provided before executing the command.");
        }

        try
        {
            action(_interaction!, parameter);
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
        if (parameter is TParameter t)
        {
            Execute(t);
        }
        else if (parameter is null)
        {
            if (typeof(TParameter).IsValueType)
            {
                throw new ArgumentException(
                    $"The command expects a value type {typeof(TParameter)}, but null was actually passed in.");
            }

            Execute(default(TParameter)!);
        }
        else
        {
            throw new ArgumentException(
                $"The parameter type does not match, expected type {typeof(TParameter)}, actual type {parameter.GetType()}.");
        }
    }

    /// <summary>
    /// UI 层请调用此方法以提供用户交互的具体实现。
    /// </summary>
    /// <param name="interaction">交互式命令的交互细节，请自行定义一个记录（record）继承自 <see cref="InteractiveCommandInteraction"/>。</param>
    /// <returns>构造器模式。</returns>
    public InteractiveCommand<TInteraction, TParameter> ProvideInteraction(TInteraction interaction)
    {
        _interaction = interaction;
        return this;
    }
}
