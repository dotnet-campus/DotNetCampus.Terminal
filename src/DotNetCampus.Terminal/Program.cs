using DotNetCampus.Terminal.Views;
using Terminal.Gui.App;

Application.Init();

try
{
    Application.Run(new MainWindow());
}
finally
{
    Application.Shutdown();
}
