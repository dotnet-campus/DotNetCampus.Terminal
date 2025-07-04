using DotNetCampus.Terminal.Configurations;
using DotNetCampus.Terminal.Framework;
using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampusTerminalViews;
using Terminal.Gui.App;

Application.Init();

try
{
    new AppBuilder()
        .UseContainer(s => s
            .AddSingleton<ConfigurationManager>());
    Application.Run(new RootView());
}
finally
{
    Application.Shutdown();
}
