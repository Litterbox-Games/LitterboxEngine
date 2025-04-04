using Arch.Core.Utils;
using Client.Services.World;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Client.Systems;
using Common.Components;
using Common.Systems;

namespace Client.Host.Registrars;

/// <summary>
///     Registers any game systems that all client hosts share.
/// </summary>
[RegistrarMode(EGameMode.Client | EGameMode.Host | EGameMode.SinglePlayer)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.Low)]
public class ClientSystemRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        container.RegisterSingleton<WorldRenderService, WorldRenderService>();
        
        // Components
        // TODO: better way of registering components, they should be order independent so we probably don't need to register them explicitly
        ComponentRegistry.Add<Position>(); 
        
        // Systems
        container.RegisterSingleton<CameraSystem, CameraSystem>();
        container.RegisterSingleton<PlayerControlSystem, PlayerControlSystem>();
        container.RegisterSingleton<EntityRenderSystem, EntityRenderSystem>();
        container.RegisterSingleton<MovementSystem, MovementSystem>();
    }                                                                 
}