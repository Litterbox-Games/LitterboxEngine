using Arch.Core;
using Arch.Core.Extensions;
using Client.Services.Network;
using Common.Components;
using Common.Services.Entities;
using Common.Services.Entities.Events;
using Common.Services.Events;
using Common.Services.Players;

namespace Client.Services.Entities;

public class ClientEntityService : IEntityService
{
    private readonly QueryDescription _networkEntities = new QueryDescription().WithAll<Networked>();
    
    public Arch.Core.World Entities { get; } = Arch.Core.World.Create();

    private readonly IPlayerService _playerService;
    private readonly EventService _eventService;

    public ClientEntityService(EventService eventService, IPlayerService playerService)
    {
        _playerService = playerService;
        _eventService = eventService;

        _eventService.Handle<EntitySpawnEvent>(OnEntitySpawn);
        _eventService.Handle<EntityDespawnEvent>(OnEntityDespawn);
    }

    private void OnEntitySpawn(EntitySpawnEvent e)
    {
        switch (e.EntityType)
        {
            // TODO: Better way to lookup entity by `EntityType` and cast to the correct entity.
            case 0: // Player
            {
                var entity = Entities.Create(
                    new Networked { OwnerId = e.EntityOwner, NetworkId = e.EntityId, EntityType = e.EntityType },
                    
                    new Player(),
                    new Position(e.EntityPosition));

                if (e.EntityOwner == _playerService.PlayerId)
                {
                    entity.Add<Velocity>();
                    entity.Add<PlayerControls>();
                    entity.Add<CameraFollow>();
                }
                       
            
                _eventService.Emit(new EntityCreatedEvent { Entity = entity });
                break;
            }
            case 1: // Mob
            {
                var entity = Entities.Create(
                    new Networked { OwnerId = e.EntityOwner, NetworkId = e.EntityId, EntityType = e.EntityType },
                    new Position(e.EntityPosition));
            
                _eventService.Emit(new EntityCreatedEvent { Entity = entity });
                break;
            }
        }
    }

    private void OnEntityDespawn(EntityDespawnEvent e)
    {
        Entities.Query(_networkEntities,( 
            Entity entity, 
            ref Networked network 
        ) => { 
            if (network.NetworkId != e.EntityId) return;
            _eventService.Emit(new EntityDestroyedEvent{ Entity = entity });
            Entities.Destroy(entity);
        });
    }
}