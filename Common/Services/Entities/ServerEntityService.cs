using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Common.Components;
using Common.Services.Entities.Messages;
using Common.Services.Events;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.Players.Messages;

namespace Common.Services.Entities;

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
        
        _eventService.Handle<PlayerConnectMessage>(OnPlayerConnect);
        _eventService.Handle<PlayerDisconnectMessage>(OnPlayerDisconnect);
        // _eventService.Handle<ServerStartEvent>(OnServerStart);
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
        // _eventService.Emit(new EntityCreatedEvent { Entity = entity });
    }

    public void DespawnEntity(Entity entity)
    {
        if (!entity.Has<Networked>())
        {
            Entities.Destroy(entity);
            return;
        }                        
        
        Entities.Destroy(entity);
        
        _eventService.Emit(new EntityDespawnEvent { EntityId = entity.Get<Networked>().NetworkId });
        // _eventService.Emit(new EntityDestroyedEvent { Entity = entity });
    }

    private void OnPlayerConnect(PlayerConnectMessage e)
    {
        var entity = Entities.Create(
            new Networked { OwnerId = e.NetworkPlayer!.PlayerId, NetworkId = (ulong) _random.Next(), EntityType = 0 },
            new Player(), 
            new Position(Vector2.Zero));
        
        SpawnEntity(entity);
        
        Entities.Query(_players, ( 
            ref Networked network, 
            ref Position position
        ) => { 
            if (network.OwnerId == e.NetworkPlayer!.PlayerId) return;
            
            var entitySpawnMessage = new EntitySpawnEvent
            {
                EntityId = network.NetworkId,
                EntityOwner = network.OwnerId,
                EntityType = network.EntityType,
                EntityPosition = position.Current,
                Receivers = serverPlayer => serverPlayer.PlayerId == e.NetworkPlayer!.PlayerId
            };

            _eventService.Emit(entitySpawnMessage);
        });
    }

    private void OnPlayerDisconnect(PlayerDisconnectMessage e)
    {
        Entities.Query(_networkEntities,( 
            Entity entity, 
            ref Networked network 
        ) => { 
            if (network.OwnerId != e.PlayerId) return;
            DespawnEntity(entity);
        });
    }

    // If player is hosting, spawn them an entity as if they just connected to a server.
    // TODO: just handle in ClientEntityService
    // private void OnServerStart(ServerStartEvent _)
    // {
    //     if (!_playerService.Players.Any()) return;
    //     OnPlayerConnect(_playerService.Players.First(x => x.PlayerId == _playerService.PlayerId));
    // }
}