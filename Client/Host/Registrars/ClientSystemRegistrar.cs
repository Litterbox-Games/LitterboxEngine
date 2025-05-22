using Autofac;
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
    public void RegisterServices(ContainerBuilder container)
    {
        container.RegisterType<WorldRenderService>().AsSelf().SingleInstance();
        
        container.RegisterType<CameraSystem>().AsSelf().SingleInstance();
        container.RegisterType<PlayerControlSystem>().AsSelf().SingleInstance();
        container.RegisterType<EntityRenderSystem>().AsSelf().SingleInstance();
    }                                                                 
}