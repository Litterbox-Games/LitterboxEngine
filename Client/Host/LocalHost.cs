using Client.Core;
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
    // Engine
    public IContainer EngineContainer { get; }
    public List<(EPriority, IUpdatable)> EngineUpdatables { get; }
    
    // Game
    public IContainer? GameContainer  { get; set; }
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];
    
    public LocalHost()
    {
        EngineContainer = new Container();
        EngineContainer.RegisterServices(EGameMode.Client | EGameMode.SinglePlayer | EGameMode.Host, ELifetime.Engine);
        EngineUpdatables = EngineContainer.RegisterUpdatables();
    }

    public void Start(EGameMode gameMode)
    {
        GameContainer = EngineContainer.CreateChildContainer();
        GameContainer.RegisterServices(gameMode, ELifetime.Game);
        GameUpdatables = GameContainer.RegisterUpdatables();
        
        GameInputables = GameContainer.RegisterInputables();
        GameDrawables = GameContainer.RegisterDrawables();

        (this as IServerHost).StartServer(7777);
        (this as IServerHost).SpawnServerPlayer();
    }

    public void Stop()
    {
        (this as IServerHost).StopServer();
        GameContainer?.Dispose();
        GameContainer = null;
        GameUpdatables = [];
        GameInputables = [];
        GameDrawables = [];
    }
    
    public void Update(float deltaTime)
    {
        EngineUpdatables.ForEach(updatable => updatable.Item2.Update(deltaTime));
        GameUpdatables.ForEach(updatable => updatable.Item2.Update(deltaTime));
    }
    
    public void Input(InputService input)
    {
        GameInputables.ForEach(inputable => inputable.Input(input));
    }
    
    public void Draw(float deltaTime, RendererService renderer)
    {
        GameDrawables.ForEach(drawable => drawable.Draw(deltaTime, renderer));
    }
    

    public void Dispose()
    { 
        EngineContainer.Dispose();
        GC.SuppressFinalize(this);
    }

}