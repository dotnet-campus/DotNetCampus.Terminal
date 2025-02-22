using Avalonia.Collections;

namespace DotNetCampus.Terminal.ViewModels;

public class ConnectionsViewModel
{
    public AvaloniaList<TabModel> CurrentConnections { get; } =
    [
        new ConnectionModel("早期版本连接"),
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
    public override string Name => "即将开放新建";
}
