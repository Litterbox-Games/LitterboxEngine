using Autofac;
using Autofac.Core;
using Client.Core.Extensions;
using Client.Graphics;
using Client.Services.Network;
using Common.Core;
using Common.Host;
using Common.Core.Extensions;

namespace Client.Host;

/// <summary>
///     The host used to represent the client game state.
/// </summary>
public class ClientHost : IClientHost
{
    // Game
    public ILifetimeScope GameContainer  { get; set; } = null!;

    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];

    public void Start(Container engineContainer, EGameMode gameMode)
    {
        GameContainer = engineContainer.BeginLifetimeScope(builder =>
        {
            builder.RegisterServices(gameMode, ELifetime.Game);
        });

        GameUpdatables = engineContainer.RegisterUpdatables();
        GameInputables = engineContainer.RegisterInputables();
        GameDrawables = engineContainer.RegisterDrawables();

        var networkService = GameContainer.Resolve<ClientNetworkService>();
        networkService.Connect("127.0.0.1", 7777);
    }

    public void Stop()
    {
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