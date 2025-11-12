using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DotNetCampus.Terminal.Framework.DependencyInjection;
using DotNetCampus.Terminal.ViewModels;
using DotNetCampus.Terminal.Views;

namespace DotNetCampus.Terminal;

public partial class App : Application
{
    private IServiceScope? _serviceScope;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceProvider = Container.Current;
        _serviceScope = serviceProvider.CreateScope();
        using var attach = _serviceScope.AttachScopeToRoot();

        if (ApplicationLifetime is ISingleViewApplicationLifetime singleLifetime)
        {
            singleLifetime.MainView = new MainView
            {
                DataContext = _serviceScope.EnsureGet<MainViewModel>(),
            };
        }
        else if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.MainWindow = new MainWindow
            {
                DataContext = _serviceScope.EnsureGet<MainViewModel>(),
            };
            desktopLifetime.Exit += (sender, args) => _serviceScope.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
