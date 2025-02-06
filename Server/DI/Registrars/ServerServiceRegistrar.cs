using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Player;
using Common.Resource;
using Common.World;
using Common.World.Generation;

namespace Server.DI.Registrars;

/// <summary>
///     Registers all dedicated server services.
/// </summary>
[RegistrarMode(EGameMode.Dedicated), RegistrarPriority(EPriority.High)]
public class ServerServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        container.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        
        container.RegisterSingleton<IResourceService, ServerResourceService>();
        
        container.RegisterSingleton<INetworkService, ServerNetworkService>();
        container.RegisterSingleton<IPlayerService, ServerPlayerService>();
        container.RegisterSingleton<IEntityService, ServerEntityService>();
        container.RegisterSingleton<MobControllerService, MobControllerService>();
        container.RegisterSingleton<IWorldGenerator, EarthGenerator>("earth");
        container.RegisterSingleton<IWorldService, ServerWorldService>();
    }
}