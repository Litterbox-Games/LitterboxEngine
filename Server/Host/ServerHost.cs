using Common.Core;
using Common.Host;

namespace Server.Host;

/// <summary>
///     The host for dedicated servers without a local client.
/// </summary>
public class ServerHost : IServerHost
{
    // Engine
    public IContainer EngineContainer { get; }
    public List<(EPriority, IUpdatable)> EngineUpdatables { get; }
    
    // Game
    public IContainer? GameContainer  { get; set; }
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    
    public ServerHost()
    {
        EngineContainer = new Container();
        EngineContainer.RegisterServices(EGameMode.Dedicated, ELifetime.Engine);
        EngineUpdatables = EngineContainer.RegisterUpdatables();
    }
    
    public void Start(EGameMode gameMode)
    {
        GameContainer = EngineContainer.CreateChildContainer();
        GameContainer.RegisterServices(gameMode, ELifetime.Game);
        GameUpdatables = GameContainer.RegisterUpdatables();
        
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
        EngineUpdatables.ForEach(updatable => updatable.Item2.Update(deltaTime));
        GameUpdatables.ForEach(updatable => updatable.Item2.Update(deltaTime));
    }

    public void Dispose()
    {
        EngineContainer.Dispose();
        GC.SuppressFinalize(this);
    }
}