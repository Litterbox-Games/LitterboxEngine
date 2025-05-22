using Arch.Core.Utils;
using Autofac;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Host;
using Common.Services.Block;
using Common.Services.Entities;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.World;
using Common.Services.World.Generation;

namespace Server.Host.Registrars;


[RegistrarMode(EGameMode.Dedicated)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.High)]
public class ServerRegistrar: IServiceRegistrar
{
    public void RegisterServices(ContainerBuilder container)
    {
        // TODO: better way of registering components, they should be order independent so we probably don't need to register them explicitly
        ComponentRegistry.Add<Position>();
        
        container.RegisterType<BlockRegistry>().AsSelf().SingleInstance();
        
        container.RegisterType<ServerNetworkService>().As<NetworkService>().AsSelf().SingleInstance();
        container.RegisterType<ServerPlayerService>().As<IPlayerService>().AsSelf().SingleInstance();
        container.RegisterType<ServerEntityService>().As<IEntityService>().AsSelf().SingleInstance();
        container.RegisterType<EarthGenerator>().As<IWorldGenerator>().AsSelf().SingleInstance();
        container.RegisterType<ServerWorldService>().As<IWorldService>().AsSelf().SingleInstance();
        
        container.RegisterType<MovementService>().AsSelf().SingleInstance();
        container.RegisterType<MobControllerService>().AsSelf().SingleInstance();
    }
}