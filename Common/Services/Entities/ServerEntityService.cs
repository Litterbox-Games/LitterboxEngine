using System.Numerics;
using Common.Components;
using Common.Entities.Components;
using Common.Entities.Messages;
using Common.Network;
using Common.Players;
using Arch.Core;
using Arch.Core.Extensions;

namespace Common.Entities;

public class ServerEntityService: IEntityService
{
    private readonly QueryDescription _networkEntities = new QueryDescription().WithAll<Networked>();
    
    // TODO: we shouldn't really require Position here
    private readonly QueryDescription _players = new QueryDescription().WithAll<Networked, Position>();
    
    public event Action<Entity>? EventOnEntitySpawn;
    public event Action<Entity>? EventOnEntityDespawn;

    public Arch.Core.World Entities { get; } = Arch.Core.World.Create();
    
    private readonly IServerNetworkService _network;
    private readonly IPlayerService _playerService;
    
    private readonly Random _random = new();
    
    public ServerEntityService(IServerNetworkService network, IPlayerService playerService)
    {
        _network = network;
        _playerService = playerService;
        
        _network.EventOnPlayerConnect += OnPlayerConnect;
        _network.EventOnPlayerDisconnect += OnPlayerDisconnect;
        _network.EventOnStartListen += OnStartListen;
    }

    public void SpawnEntity(Entity entity)
    {
        if (!entity.Has<Position, Networked>()) return;
        
        var position = entity.Get<Position>();
        var network = entity.Get<Networked>();
        
        var entitySpawnMessage = new EntitySpawnMessage()
        {
            EntityType = network.EntityType,
            EntityId = network.NetworkId,
            EntityOwner = network.OwnerId,
            EntityPosition = position.Current
        };
        
        _network.SendToAllPlayers(entitySpawnMessage);
        EventOnEntitySpawn?.Invoke(entity);
    }

    public void DespawnEntity(Entity entity)
    {
        if (!entity.Has<Networked>())
        {
            Entities.Destroy(entity);
            return;
        }                        
        
        var network = entity.Get<Networked>();
        
        Entities.Destroy(entity);
        
        var entityDeleteMessage = new EntityDespawnMessage
        {
            EntityId = network.NetworkId
        };
        
        _network.SendToAllPlayers(entityDeleteMessage);
        
        EventOnEntityDespawn?.Invoke(entity);
    }

    private void OnPlayerConnect(ServerPlayer player)
    {
        var entity = Entities.Create(
            new Networked { OwnerId = player.PlayerId, NetworkId = (ulong) _random.Next(), EntityType = 0 },
            new Player(), 
            new Position(Vector2.Zero));
        
        SpawnEntity(entity);
        
        Entities.Query(_players, ( 
            ref Networked network, 
            ref Position position
        ) => { 
            if (network.OwnerId == player.PlayerId) return;
            
            var entitySpawnMessage = new EntitySpawnMessage
            {
                EntityId = network.NetworkId,
                EntityOwner = network.OwnerId,
                EntityType = network.EntityType,
                EntityPosition = position.Current
            };

            _network.SendToPlayer(entitySpawnMessage, player); 
        });
    }

    private void OnPlayerDisconnect(ServerPlayer player)
    {
        Entities.Query(_networkEntities,( 
            Entity entity, 
            ref Networked network 
        ) => { 
            if (network.OwnerId != player.PlayerId) return;
            DespawnEntity(entity);
        });
    }

    // If player is hosting, spawn them an entity as if they just connected to a server.
    private void OnStartListen()
    {
        if (!_network.Players.Any()) return;
        OnPlayerConnect(_network.Players.First(x => x.PlayerId == _playerService.PlayerId));
    }
}