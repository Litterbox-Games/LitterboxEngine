using Autofac;
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
    public EGameMode GameMode => EGameMode.Dedicated;
    public ILifetimeScope GameScope  { get; set; } = null!;
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    
    public void Start(ILifetimeScope engineScope)
    {
        GameScope = engineScope.BeginLifetimeScope(builder =>
        {
            builder.RegisterServices(GameMode, ELifetime.Game);
        });
        
        GameScope.Resolve<RootLoggingService>().RefreshLoggers(GameScope);
        
        GameUpdatables = GameScope.RegisterUpdatables();
        
        (this as IServerHost).StartServer(7777);
    }
    
    public void Stop()
    {
        GameScope.Dispose();
        GameUpdatables = [];
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}