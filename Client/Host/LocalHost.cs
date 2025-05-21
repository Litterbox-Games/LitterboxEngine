using Autofac;
using Autofac.Core;
using Client.Core.Extensions;
using Client.Graphics;
using Common.Core;
using Common.Core.Extensions;
using Common.Host;
using Common.Services.Logging;

namespace Client.Host;

/// <summary>
///     The host used for single player or for local hosting.
/// </summary>
public class LocalHost : IClientHost, IServerHost
{
    // Game
    public ILifetimeScope GameContainer { get; set; } = null!;
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];

    public void Start(Container engineContainer, EGameMode gameMode)
    {
        GameContainer = engineContainer.BeginLifetimeScope(builder =>
        {
            builder.RegisterServices(gameMode, ELifetime.Game);
        });
        
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