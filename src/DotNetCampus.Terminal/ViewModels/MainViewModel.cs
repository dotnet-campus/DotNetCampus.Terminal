namespace DotNetCampus.Terminal.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ConnectionsViewModel Tabs { get; } = new();
}
