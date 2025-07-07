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
                .StartWithConsoleLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UseConsolonia()
                .UseAutoDetectedConsole()
                .UseContainerServices()
                .LogToException();
        }
    }
}
