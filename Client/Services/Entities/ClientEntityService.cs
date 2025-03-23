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

    private void OnEntitySpawnMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var castedMessage = (EntitySpawnMessage) message;

        switch (castedMessage.EntityType)
        {
            // TODO: Better way to lookup entity by `EntityType` and cast to the correct entity.
            case 0: // Player
            {
                var entity = Entities.Create(
                    new Networked { OwnerId = castedMessage.EntityOwner, NetworkId = castedMessage.EntityId, EntityType = castedMessage.EntityType },
                    new Player(),
                    new Position(castedMessage.EntityPosition));

                if (castedMessage.EntityOwner == _playerService.PlayerId) 
                    entity.Add<PlayerControls>();   
            
                EventOnEntitySpawn?.Invoke(entity);
                break;
            }
            case 1: // Mob
            {
                var entity = Entities.Create(
                    new Networked { OwnerId = castedMessage.EntityOwner, NetworkId = castedMessage.EntityId, EntityType = castedMessage.EntityType },
                    new Position(castedMessage.EntityPosition));
            
                EventOnEntitySpawn?.Invoke(entity);
                break;
            }
        }
    }

    private void OnEntityDespawnMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var castedMessage = (EntityDespawnMessage) message;
        
        Entities.Query(_networkEntities,( 
            ref Entity entity, 
            ref Networked network 
        ) => { 
            if (network.NetworkId != castedMessage.EntityId) return;
            Entities.Destroy(entity);
            EventOnEntityDespawn?.Invoke(entity);
        });
    }
}