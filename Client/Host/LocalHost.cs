using Autofac.Core;
using Client.Graphics;
using Client.Host.Extensions;
using Common.Core;
using Common.Host;
using Common.Host.Extensions;
using MoreLinq;

namespace Client.Host;

/// <summary>
///     The host used for single player or for local hosting.
/// </summary>
public class LocalHost : IClientHost, IServerHost
{
    // Game
    public Container? GameContainer { get; set; }
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];

    public void Start(Container engineContainer, EGameMode gameMode)
    {
        GameContainer = engineContainer.CreateChildContainer();
        GameContainer.RegisterServices(gameMode, ELifetime.Game);
        
        this.RegisterUpdatables();
        this.RegisterInputables();
        this.RegisterDrawables();
        
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