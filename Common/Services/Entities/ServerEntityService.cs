using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Common.Components;
using Common.Core;
using Common.Services.Entities.Events;
using Common.Services.Events;
using Common.Services.Players;
using Common.Services.Players.Events;

namespace Common.Services.Entities;

[Game(EMode.Host)]
[As<IEntityService>]
public class ServerEntityService: IEntityService
{
    private readonly QueryDescription _networkEntities = new QueryDescription().WithAll<Networked>();
    
    // TODO: we shouldn't really require Position here
    private readonly QueryDescription _players = new QueryDescription().WithAll<Networked, Position>();

    public Arch.Core.World Entities { get; } = Arch.Core.World.Create();
    
    private readonly EventService _eventService;
    private readonly IPlayerService _playerService;
    
    private readonly Random _random = new();
    
    public ServerEntityService(EventService eventService, IPlayerService playerService)
    {
        _eventService = eventService;
        _playerService = playerService;
        
        _eventService.Handle<PlayerConnectEvent>(OnPlayerConnect);
        _eventService.Handle<PlayerDisconnectEvent>(OnPlayerDisconnect);
    }

    public void SpawnEntity(Entity entity)
    {
        if (!entity.Has<Position, Networked>()) return;
        
        var position = entity.Get<Position>();
        var network = entity.Get<Networked>();
        
        _eventService.Emit(new EntitySpawnEvent
        {
            EntityType = network.EntityType,
            EntityId = network.NetworkId,
            EntityOwner = network.OwnerId,
            EntityPosition = position.Current
        });
        
        _eventService.Emit(new EntityCreatedEvent { Entity = entity });
    }

    public void DespawnEntity(Entity entity)
    {
        if (!entity.TryGet(out Networked networked))
        {
            Entities.Destroy(entity);
            return;
        }                        
        
        Entities.Destroy(entity);
        
        _eventService.Emit(new EntityDespawnEvent { EntityId = networked.NetworkId });
    }

    private void OnPlayerConnect(PlayerConnectEvent e)
    {
        var entity = Entities.Create(
            new Networked { OwnerId = e.NetworkPlayer.PlayerId, NetworkId = (ulong) _random.Next(), EntityType = 0 },
            new Player(), 
            new Position(Vector2.Zero));
        
        // Local player
        if (e.NetworkPlayer.PlayerId == _playerService.PlayerId)
        {
            entity.Add<Velocity>();
            entity.Add<PlayerControls>();
            entity.Add<CameraFollow>();
        }
        
        SpawnEntity(entity);
        
        Entities.Query(_players, ( 
            ref Networked network, 
            ref Position position
        ) => { 
            if (network.OwnerId == e.NetworkPlayer.PlayerId) return;
            
            _eventService.Emit(new EntitySpawnEvent
            {
                EntityId = network.NetworkId,
                EntityOwner = network.OwnerId,
                EntityType = network.EntityType,
                EntityPosition = position.Current,
                Receivers = serverPlayer => serverPlayer.PlayerId == e.NetworkPlayer.PlayerId
            });
        });
    }

    private void OnPlayerDisconnect(PlayerDisconnectEvent e)
    {
        Entities.Query(_networkEntities,( 
            Entity entity, 
            ref Networked network 
        ) => { 
            if (network.OwnerId != e.PlayerId) return;
            DespawnEntity(entity);
        });
    }
    
    public void Dispose()
    {
        Arch.Core.World.Destroy(Entities);
        
        _eventService.Unhandle<PlayerConnectEvent>(OnPlayerConnect);
        _eventService.Unhandle<PlayerDisconnectEvent>(OnPlayerDisconnect);
        
        GC.SuppressFinalize(this);
    }
}