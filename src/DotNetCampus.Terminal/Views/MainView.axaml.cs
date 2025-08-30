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
            var topLevel = TopLevel.GetTopLevel(this)!;
            if (topLevel is Window window)
            {
                window.Title = "DotNetCampus Terminal";
            }

            ViewModel.ReloadDevicesCommand.Execute(null);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            var lifetime = Application.Current!.ApplicationLifetime as IControlledApplicationLifetime;
            lifetime!.Shutdown();
        }

        private void FavoriteButton_IsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            // var toggleButton = (ToggleButton)sender!;
            // var info = (IRemoteDeviceInfo)toggleButton.DataContext!;
            // var isChecked = toggleButton.IsChecked is true;
            // if (isChecked && !ViewModel.FavoriteDevices.Contains(deviceInfo))
            // {
            //     ViewModel.FavoriteDevices.Add(deviceInfo);
            // }
            // else if (!isChecked && ViewModel.FavoriteDevices.Contains(deviceInfo))
            // {
            //     ViewModel.FavoriteDevices.Remove(deviceInfo);
            // }
        }
    }
}
