using Client.Graphics;
using Common.Core;
using Common.Host;
using Common.Services.Players;

namespace Client.Host;

/// <summary>
///     The host used for single player or for local hosting.
/// </summary>
public class LocalHost : IClientHost, IServerHost
{
    public IContainer Container { get; }
    public List<IInputable> Inputables { get; } = [];
    public List<(EPriority, IUpdatable)> Updatables { get; } = [];
    public List<IDrawable> Drawables { get; } = [];
    
    public LocalHost(bool singlePlayer)
    {
        Container = new Container(singlePlayer ? EGameMode.SinglePlayer : EGameMode.Host);
        Container.RegisterServices();
        (this as IHost).RegisterUpdatables();
        (this as IClientHost).RegisterInputables();
        (this as IClientHost).RegisterDrawables();
        
        // Warm Service Singletons
        Container.Resolve<IPlayerService>();

        (this as IServerHost).StartServer(7777);
    }

    public void Dispose()
    { 
        Container.Dispose(); 
        GC.SuppressFinalize(this);
    }

}