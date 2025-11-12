using Avalonia;
using DotNetCampus.Terminal.Modules;

namespace DotNetCampus.Terminal;

public static class Program
{
    /// <summary>
    /// Initialization code. Don't use any Avalonia, third-party APIs or any
    /// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    /// yet and stuff might break.
    /// </summary>
    /// <param name="args"></param>
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// 此方法会被 Avalonia 设计器调用，请不要删除，可以在此方法中初始化设计时所需的数据
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
    {
        var appBuilder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseContainerServices();
        // 解除注释以调试 XAML 设计器。
        // Debugger.Launch();
        return appBuilder;
    }
}
