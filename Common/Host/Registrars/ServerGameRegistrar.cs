using Common.Core;
using Common.Core.Attributes;
using Common.Services.Block;
using Common.Services.Entities;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.World;
using Common.Services.World.Generation;

namespace Common.Host.Registrars;

/// <summary>
///     Registers any game services all server hosts share.
/// </summary>
[RegistrarMode(EGameMode.Dedicated | EGameMode.Host | EGameMode.SinglePlayer)]
[RegistrarLifetime(ELifetime.Game)]
[RegistrarPriority(EPriority.High)]
public class ServerGameRegistrar: IServiceRegistrar
{
    public void RegisterServices(IContainer container)
    {
        container.RegisterSingleton<BlockRegistry, BlockRegistry>();
        
        container.RegisterSingleton<NetworkService, ServerNetworkService>();
        container.RegisterSingleton<IPlayerService, ServerPlayerService>();
        
        container.RegisterSingleton<IEntityService, ServerEntityService>();
        
        container.RegisterSingleton<IWorldGenerator, EarthGenerator>("earth");
        container.RegisterSingleton<IWorldService, ServerWorldService>();
    }
}