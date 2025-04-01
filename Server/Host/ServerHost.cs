using Common.Core;
using Common.Host;

namespace Server.Host;

/// <summary>
///     The host for dedicated servers without a local client.
/// </summary>
public class ServerHost : IServerHost
{
    public List<(EPriority, IUpdatable)> Updatables { get; } = [];
    public IContainer Container { get; }
    
    public ServerHost()
    {
        Container = new Container(EGameMode.Dedicated);
        Container.RegisterServices();
        (this as IHost).RegisterUpdatables();
        (this as IServerHost).StartServer(7777);
    }

    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }
}