using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Generation;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Player;
using Common.Resource;
using Common.World;

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
        
        host.RegisterSingleton<INetworkService, ServerNetworkService>();
        host.RegisterSingleton<IPlayerService, ServerPlayerService>();
        host.RegisterSingleton<IEntityService, ServerEntityService>();
        host.RegisterSingleton<MobControllerService, MobControllerService>();
        host.RegisterSingleton<IWorldGenerator, EarthGenerator>("earth");
        host.RegisterSingleton<IWorldService, ServerWorldService>();
    }
}