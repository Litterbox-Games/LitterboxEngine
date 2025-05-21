using Autofac;
using Autofac.Core;
using Common.Core;
using Common.Host;
using Common.Host.Extensions;

namespace Server.Host;

/// <summary>
///     The host for dedicated servers without a local client.
/// </summary>
public class ServerHost : IServerHost
{
    // Game
    public Container? GameContainer  { get; set; }
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    
    public void Start(IContainer engineContainer, EGameMode gameMode)
    {
        GameContainer = engineContainer.CreateChildContainer();
        GameContainer.RegisterServices(gameMode, ELifetime.Game);
        
        this.RegisterUpdatables();
        
        (this as IServerHost).StartServer(7777);
    }
    
    public void Stop()
    {
        GameContainer?.Dispose();
        GameContainer = null;
        GameUpdatables = [];
    }
    
    public void Update(float deltaTime)
    {
        GameUpdatables.ForEach(updatable => updatable.Item2.Update(deltaTime));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}