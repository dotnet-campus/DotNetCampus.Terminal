using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using DotNetCampus.Terminal.ViewModels;

namespace DotNetCampus.Terminal.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private MainViewModel ViewModel => (MainViewModel)DataContext!;

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            ViewModel.ReloadDevicesCommand.Execute(null);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            var lifetime = Application.Current!.ApplicationLifetime as IControlledApplicationLifetime;
            lifetime!.Shutdown();
        }
    }
}
