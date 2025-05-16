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
    // Engine
    public IContainer EngineContainer { get; }
    public List<(EPriority, IUpdatable)> EngineUpdatables { get; }
    
    // Game
    public IContainer? GameContainer  { get; set; }
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];
    
    public ClientHost()
    {
        EngineContainer = new Container();
        EngineContainer.RegisterServices(EGameMode.Client | EGameMode.SinglePlayer | EGameMode.Host, ELifetime.Engine);
        EngineUpdatables = (this as IHost).RegisterUpdatables(EngineContainer);
    }
    
    public void Start(EGameMode gameMode)
    {
        GameContainer = EngineContainer.CreateChildContainer();
        GameContainer.RegisterServices(gameMode, ELifetime.Game);
        GameUpdatables = (this as IHost).RegisterUpdatables(GameContainer);
        
        GameInputables = (this as IClientHost).RegisterInputables(GameContainer);
        GameDrawables = (this as IClientHost).RegisterDrawables(GameContainer);

        var networkService = GameContainer.Resolve<ClientNetworkService>();
        networkService.Connect("127.0.0.1", 7777);
    }

    public void Stop()
    {
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