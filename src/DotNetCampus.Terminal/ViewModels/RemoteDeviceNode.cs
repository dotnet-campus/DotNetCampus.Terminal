using DotNetCampus.Terminal.Modules.Configurations;
using DotNetCampus.Terminal.Modules.Configurations.Models;

namespace DotNetCampus.Terminal.ViewModels;

public interface IRemoteDeviceNode
{
    IReadOnlyList<IRemoteDeviceNode> Children { get; }

    public static RemoteDeviceGroupNode From(RemoteDeviceGroup group)
    {
        return new RemoteDeviceGroupNode(group)
        {
            Children = group.Devices.Select(From).ToList(),
        };
    }

    public static RemoteDeviceInfoNode From(IRemoteDeviceInfo info)
    {
        return info switch
        {
            SshRemoteDeviceInfo sshInfo => new SshRemoteDeviceInfoNode(sshInfo),
            _ => new RemoteDeviceInfoNode(info),
        };
    }
}

public record RemoteDeviceGroupNode(RemoteDeviceGroup Info) : IRemoteDeviceNode
{
    public required IReadOnlyList<IRemoteDeviceNode> Children { get; init; }
}

public record RemoteDeviceInfoNode(IRemoteDeviceInfo Info) : IRemoteDeviceNode
{
    public IReadOnlyList<IRemoteDeviceNode> Children { get; } = [];
}

public record SshRemoteDeviceInfoNode : RemoteDeviceInfoNode
{
    public SshRemoteDeviceInfoNode(SshRemoteDeviceInfo info) : base(info)
    {
    }
}
