using System.Diagnostics;
using Avalonia;
using DotNetCampus.Logging;
using DotNetCampus.Terminal.FileSync;
using DotNetCampus.Terminal.Framework;
using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampus.Terminal.Modules.Configurations;
using DotNetCampus.Terminal.ViewModels;

namespace DotNetCampus.Terminal.Modules;

public static class Startup
{
    public static AppBuilder UseContainerServices(this AppBuilder appBuilder)
    {
        return appBuilder.UseContainer(c => c
            .AddLazyServices(sc => sc
                .AddSingleton<ConfigurationManager>()
                .AddSingleton<IFileSyncService>(_ => new FileSyncService())
                .AddSingleton<ILogger>(_ => new LoggerBuilder()
                    .WithLevel(LogLevel.Information)
                    .AddWriter(new EmptyLogger())
                    .Build()
                    .IntoGlobalStaticLog()))
            .AddLazyServices(sc => sc
                .AddScoped(s => new MainViewModel(s))
                .AddScoped(s => new StatusBarViewModel(s))
                .AddSingleton<StatusTipViewModel>()
            )
        );
    }
}

public class EmptyLogger : ILogger
{
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Debug.WriteLine(formatter(state, exception));
    }
}
