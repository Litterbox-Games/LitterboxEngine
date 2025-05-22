using Autofac;
using Autofac.Core;
using Common.Core;
using Common.Core.Extensions;
using Common.Host;
using Common.Services.Logging;

namespace Server.Host;

/// <summary>
///     The host for dedicated servers without a local client.
/// </summary>
public class ServerHost : IServerHost
{
    // Game
    public ILifetimeScope GameContainer  { get; set; } = null!;
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    
    public void Start(Container engineContainer, EGameMode gameMode)
    {
        GameContainer = engineContainer.BeginLifetimeScope(builder =>
        {
            builder.RegisterServices(gameMode, ELifetime.Game);
        });
        
        GameContainer.Resolve<RootLoggingService>().RefreshLoggers(GameContainer);
        
        GameUpdatables = GameContainer.RegisterUpdatables();
        
        (this as IServerHost).StartServer(7777);
    }
    
    public void Stop()
    {
        GameContainer.Dispose();
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