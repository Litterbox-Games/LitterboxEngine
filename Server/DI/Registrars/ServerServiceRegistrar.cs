using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Player;
using Common.Resource;

namespace Server.DI.Registrars;

/// <summary>
///     Registers all dedicated server services.
/// </summary>
[RegistrarMode(EGameMode.Dedicated), RegistrarPriority(EPriority.High)]
public class ServerServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(AbstractHost host)
    {
        host.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        
        host.RegisterSingleton<IResourceService, ServerResourceService>();
        
        // Register Services Here
        host.RegisterSingleton<INetworkService, ServerNetworkService>();
        host.RegisterSingleton<IPlayerService, ServerPlayerService>();
        host.RegisterSingleton<IEntityService, ServerEntityService>();
    }
}