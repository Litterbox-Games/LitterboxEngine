using Client.Core;
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
    // Game
    public IContainer? GameContainer  { get; set; }
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];
    
    public void Start(IContainer engineContainer, EGameMode gameMode)
    {
        GameContainer = engineContainer.CreateChildContainer();
        GameContainer.RegisterServices(gameMode, ELifetime.Game);
        GameUpdatables = GameContainer.RegisterUpdatables();
        
        GameInputables = GameContainer.RegisterInputables();
        GameDrawables = GameContainer.RegisterDrawables();

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
        GC.SuppressFinalize(this);
    }
}