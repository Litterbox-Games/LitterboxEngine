using Arch.Core.Utils;
using Common.DI;
using Common.DI.Attributes;
using Common.Entities;
using Common.Entities.Components;
using Common.Entities.Systems;
using Common.Host;
using Common.Logging;
using Common.Network;
using Common.Players;
using Common.Resource;
using Common.World;
using Common.World.Generation;

namespace Server.Core.Registrars;

/// <summary>
///     Registers all dedicated server services.
/// </summary>
[RegistrarMode(EGameMode.Dedicated), RegistrarPriority(EPriority.High)]
public class ServerServiceRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        // Components
        ComponentRegistry.Add<Position>();
        
        // Services
        container.RegisterSingleton<ILoggingService, ConsoleLoggingService>("console");
        
        container.RegisterSingleton<IResourceService, ServerResourceService>();
        
        container.RegisterSingleton<IServerNetworkService, ServerNetworkService>();
        container.RegisterSingleton<IPlayerService, ServerPlayerService>();
        container.RegisterSingleton<IEntityService, ServerEntityService>();
        container.RegisterSingleton<IWorldGenerator, EarthGenerator>("earth");
        container.RegisterSingleton<IWorldService, ServerWorldService>();
        
        // Systems
        container.RegisterSingleton<MobControllerSystem, MobControllerSystem>();
        container.RegisterSingleton<ServerMovementSystem, ServerMovementSystem>();
    }
}