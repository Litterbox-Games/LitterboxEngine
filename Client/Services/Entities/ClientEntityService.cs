using Arch.Core;
using Arch.Core.Extensions;
using Client.Components;
using Client.Services.Network;
using Common.Components;
using Common.Services.Entities;
using Common.Services.Entities.Messages;
using Common.Services.Network;
using Common.Services.Players;

namespace Client.Services.Entities;

public class ClientEntityService : IEntityService
{
    private readonly QueryDescription _networkEntities = new QueryDescription().WithAll<Networked>();
    
    public Arch.Core.World Entities { get; } = Arch.Core.World.Create();

    public event Action<Entity>? EventOnEntitySpawn;
    public event Action<Entity>? EventOnEntityDespawn;

    private readonly IPlayerService _playerService;

    public ClientEntityService(IClientNetworkService network, IPlayerService playerService)
    {
        _playerService = playerService;

        network.RegisterMessageHandle<EntitySpawnMessage>(OnEntitySpawnMessage);
        network.RegisterMessageHandle<EntityDespawnMessage>(OnEntityDespawnMessage);
    }

    private void OnEntitySpawnMessage(EntitySpawnMessage message, NetworkPlayer? _)
    {
        switch (message.EntityType)
        {
            // TODO: Better way to lookup entity by `EntityType` and cast to the correct entity.
            case 0: // Player
            {
                var entity = Entities.Create(
                    new Networked { OwnerId = message.EntityOwner, NetworkId = message.EntityId, EntityType = message.EntityType },
                    new Player(),
                    new Position(message.EntityPosition));

                if (message.EntityOwner == _playerService.PlayerId) 
                    entity.Add<PlayerControls>();   
            
                EventOnEntitySpawn?.Invoke(entity);
                break;
            }
            case 1: // Mob
            {
                var entity = Entities.Create(
                    new Networked { OwnerId = message.EntityOwner, NetworkId = message.EntityId, EntityType = message.EntityType },
                    new Position(message.EntityPosition));
            
                EventOnEntitySpawn?.Invoke(entity);
                break;
            }
        }
    }

    private void OnEntityDespawnMessage(EntityDespawnMessage message, NetworkPlayer? _)
    {
        Entities.Query(_networkEntities,( 
            ref Entity entity, 
            ref Networked network 
        ) => { 
            if (network.NetworkId != message.EntityId) return;
            Entities.Destroy(entity);
            EventOnEntityDespawn?.Invoke(entity);
        });
    }
}