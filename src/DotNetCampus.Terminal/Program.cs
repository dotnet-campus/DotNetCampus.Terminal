using Avalonia;
using Consolonia;
using DotNetCampus.Terminal.Modules;

namespace DotNetCampus.Terminal
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            BuildAvaloniaApp()
                // .StartWithConsoleLifetime(args)
                .StartWithClassicDesktopLifetime(args)
                ;
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                // .UseConsolonia()
                // .UseAutoDetectedConsole()
                // .UseAutoDetectConsoleColorMode()
                .UseContainerServices()
                .LogToException();
        }
    }
}
