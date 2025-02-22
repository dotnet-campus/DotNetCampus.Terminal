using Avalonia.Controls;
using DotNetCampus.Terminal.ViewModels;

namespace DotNetCampus.Terminal.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new MainViewModel();
        }
    }
}

