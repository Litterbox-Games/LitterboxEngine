using Arch.Core.Utils;
using Autofac;
using Autofac.Features.AttributeFilters;
using Client.Services.Entities;
using Client.Services.Network;
using Client.Services.Players;
using Client.Services.World;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Common.Services.Block;
using Common.Services.Entities;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.World;

namespace Client.Host.Registrars;

[RegistrarMode(EGameMode.Client)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.High)]
public class ClientRegistrar: IServiceRegistrar
{
    public void RegisterServices(ContainerBuilder container)
    {
        // TODO: better way of registering components, they should be order independent so we probably don't need to register them explicitly
        ComponentRegistry.Add<Position>();
        
        container.RegisterType<BlockRegistry>().AsSelf().SingleInstance();
        
        container.RegisterType<ClientPlayerService>().As<IPlayerService>().AsSelf().SingleInstance();
        container.RegisterType<ClientNetworkService>().As<NetworkService>().AsSelf().SingleInstance();
        container.RegisterType<ClientEntityService>().As<IEntityService>().AsSelf().SingleInstance();
        container.RegisterType<ClientWorldService>().As<IWorldService>().AsSelf().SingleInstance();
        
        container.RegisterType<WorldRenderService>().AsSelf().SingleInstance();
        container.RegisterType<EntityRenderService>().AsSelf().SingleInstance();
        container.RegisterType<CameraService>().WithAttributeFiltering().AsSelf().SingleInstance();
        
        container.RegisterType<MovementService>().AsSelf().SingleInstance();
        container.RegisterType<PlayerControlService>().AsSelf().SingleInstance();
    }
}