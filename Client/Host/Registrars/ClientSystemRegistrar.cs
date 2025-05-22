using Autofac;
using Client.Services.Entities;
using Client.Services.Players;
using Client.Services.World;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;

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
        
        container.RegisterType<CameraService>().AsSelf().SingleInstance();
        container.RegisterType<PlayerControlService>().AsSelf().SingleInstance();
        container.RegisterType<EntityRenderService>().AsSelf().SingleInstance();
    }                                                                 
}