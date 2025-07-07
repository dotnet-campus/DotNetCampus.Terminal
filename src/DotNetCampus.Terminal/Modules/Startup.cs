using Avalonia;
using DotNetCampus.Terminal.Framework;
using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampus.Terminal.Modules.Configurations;

namespace DotNetCampus.Terminal.Modules;

public static class Startup
{
    public static AppBuilder UseContainerServices(this AppBuilder appBuilder)
    {
        return appBuilder.UseContainer(s => s
            .AddSingleton<ConfigurationManager>());
    }
}
