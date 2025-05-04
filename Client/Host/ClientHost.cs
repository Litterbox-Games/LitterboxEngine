using Client.Graphics;
using Client.Services.Network;
using Common.Core;
using Common.Host;
using Common.Services.Players;

namespace Client.Host;

/// <summary>
///     The host used to represent the client game state.
/// </summary>
public class ClientHost : IClientHost
{
    public IContainer Container { get; } = new Container();
    public List<IInputable> Inputables { get; } = [];
    public List<(EPriority, IUpdatable)> Updatables { get; } = [];
    public List<IDrawable> Drawables { get; } = [];
    
    public ClientHost()
    {
        Container.RegisterServices(EGameMode.Client, ELifetime.Engine | ELifetime.Game);
        (this as IHost).RegisterUpdatables();
        (this as IClientHost).RegisterInputables();
        (this as IClientHost).RegisterDrawables();
        
        // Warm Service Singletons
        Container.Resolve<IPlayerService>();
        
        var networkService = Container.Resolve<ClientNetworkService>();
        networkService.Connect("127.0.0.1", 7777);
    }

    public void Dispose()
    {
        Container.Dispose();
        GC.SuppressFinalize(this);
    }

    
}