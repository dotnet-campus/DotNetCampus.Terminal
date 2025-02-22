using Avalonia.Collections;

namespace DotNetCampus.Terminal.ViewModels;

public class ConnectionsViewModel
{
    public AvaloniaList<TabModel> CurrentConnections { get; } =
    [
        new ConnectionModel("连接 1"),
        new ConnectionModel("连接 2"),
        new ConnectionModel("连接 3"),
        new CreateNewConnectionModel(),
    ];
}

public abstract record TabModel : BindableValueBase
{
    public abstract string Name { get; }
}

public record ConnectionModel(string Name) : TabModel
{
    public override string Name { get; } = Name;
}

public record CreateNewConnectionModel : TabModel
{
    public override string Name => "新建连接";
}
