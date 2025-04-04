using Client.Services.World;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Client.Systems;

namespace Client.Host.Registrars;

/// <summary>
///     Registers any game services that graphical clients share.
/// </summary>
[RegistrarMode(EGameMode.Client | EGameMode.Host | EGameMode.SinglePlayer)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.Low)]
public class ClientSystemRegistrar : IServiceRegistrar
{
    /// <inheritdoc />
    public void RegisterServices(IContainer container)
    {
        // Services
        container.RegisterSingleton<WorldRenderService, WorldRenderService>();
        
        // Systems
        container.RegisterSingleton<CameraSystem, CameraSystem>();
        container.RegisterSingleton<PlayerControlSystem, PlayerControlSystem>();
        container.RegisterSingleton<EntityRenderSystem, EntityRenderSystem>();
    }                                                                 
}