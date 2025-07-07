using DotNetCampus.Terminal.Framework;
using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampus.Terminal.Modules.Configurations;
using DotNetCampus.Terminal.Views;
using Terminal.Gui.App;

Application.Init();

try
{
    new AppBuilder()
        .UseContainer(s => s
            .AddSingleton<ConfigurationManager>());
    Application.Run(new RootView(Container.Current.EnsureGet<ConfigurationManager>()));
}
finally
{
    Application.Shutdown();
}
