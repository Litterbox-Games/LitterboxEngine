using System.Numerics;
using Common.Entity;
using Common.Entity.Messages;
using Common.Network;
using Common.Player;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Entity;

public class ServerEntityService : AbstractEntityService
{
    public override IEnumerable<GameEntity> Entities => _entities;
    private readonly List<GameEntity> _entities = new();
    
    public override event Action<GameEntity>? EventOnEntitySpawn;
    public override event Action<GameEntity>? EventOnEntityDespawn;
    public override event Action<GameEntity>? EventOnEntityMove;

    private ServerNetworkService _network;
    
    public ServerEntityService(INetworkService network)
    {
        _network = (ServerNetworkService)network;
        
        _network.EventOnPlayerConnect += OnPlayerConnect;
        _network.EventOnPlayerDisconnect += OnPlayerDisconnect;
        _network.EventOnStartListen += OnStartListen;

        _network.RegisterMessageHandle<EntityMoveMessage>(OnEntityMoveMessage);
    }

    /// <inheritdoc />
    public override void Update(float deltaTime)
    {
        _entities.ForEach(x =>
        {
            if ((x.OwnerId == _network.PlayerId || x.OwnerId == 0)
                && x.Position != x.LastSentPosition
                && (DateTime.Now - x.LastUpdateTime).TotalMilliseconds > 50)
            {
                var message = new EntityMoveMessage
                {
                    EntityId = x.EntityId,
                    NewPosition = x.Position
                };

                _network.SendToAllPlayers(message);

                x.LastSentPosition = x.Position;
                x.LastUpdateTime = DateTime.Now;
            }
            else
            {
                var renderTime = DateTime.Now - new TimeSpan(0, 0, 0, 0, 100);
                if (x.QueuedMovements.Count <= 1)
                    return;
                
                while (x.QueuedMovements.Count > 2 && renderTime > x.QueuedMovements.ToArray()[1].TimeStamp)
                {
                    x.QueuedMovements.Dequeue();
                }

                var arr = x.QueuedMovements.ToArray();

                var interpolationFactor = (renderTime - arr[0].TimeStamp).TotalMilliseconds /
                                          (arr[1].TimeStamp -
                                           arr[0].TimeStamp).TotalMilliseconds;

                if (interpolationFactor <= 1)
                    x.Position = Vector2.Lerp(arr[0].Position, arr[1].Position, (float) interpolationFactor);
            }

        });
    }

    /// <inheritdoc />
    public override void Draw() { }
    
     private void OnEntityMoveMessage(INetworkMessage message, Player.Player? player)
    {
        var castedMessage = (EntityMoveMessage) message;

        var entity = _entities.FirstOrDefault(x => x.EntityId == castedMessage.EntityId);

        if (entity == null) return;

        entity.QueuedMovements.Enqueue(new QueuedMovement(castedMessage.NewPosition, DateTime.Now));

        // TODO: Shouldn't need to do this unless Entity changes owners (car maybe?)
        entity.LastSentPosition = castedMessage.NewPosition;

        entity.LastUpdateTime = DateTime.Now;

        // Forward this packet to all players
        foreach (var networkPlayer in _network.Players)
        {
            if (networkPlayer != player)
            {
                _network.SendToPlayer(castedMessage, networkPlayer);
            }
        }

        EventOnEntityMove?.Invoke(entity);
    }

    private void OnPlayerConnect(ServerPlayer player)
    {
        var entity = new PlayerEntity
        {
            OwnerId = player.PlayerID,
            EntityId = player.PlayerID,
            EntitySystem = this,
            Position = Vector2.Zero
        };

        _entities.Add(entity);

        var entitySpawnMessage = new EntitySpawnMessage
        {
            EntityType = 0,
            EntityId = entity.EntityId,
            EntityOwner = entity.OwnerId,
            EntityPosition = entity.Position
        };

        _network.SendToAllPlayers(entitySpawnMessage);

        foreach (var entityToSend in _entities.Where(entityToSend => entityToSend.EntityId != player.PlayerID))
        {
            entitySpawnMessage = new EntitySpawnMessage
            {
                EntityId = entityToSend.EntityId,
                EntityOwner = entityToSend.OwnerId,
                EntityType = entityToSend.EntityType,
                EntityPosition = entityToSend.Position,
                EntityData = entityToSend.SerializeEntityData()
            };

            _network.SendToPlayer(entitySpawnMessage, player);
        }

        EventOnEntitySpawn?.Invoke(entity);
    }

    private void OnPlayerDisconnect(ServerPlayer player)
    {
        var entity = _entities.First(x => x.EntityId == player.PlayerID);

        _entities.Remove(entity);

        var entityDespawnMessage = new EntityDespawnMessage()
        {
            EntityId = player.PlayerID
        };

        foreach (var networkPlayer in _network.Players)
        {
            if (networkPlayer != player)
            {
                _network.SendToPlayer(entityDespawnMessage, networkPlayer);
            }
        }

        EventOnEntityDespawn?.Invoke(entity);
    }

    // If player is hosting, spawn them an entity as if they just connected to a server.
    private void OnStartListen()
    {
        if (_network.Players.Any())
        {
            OnPlayerConnect(_network.Players.First(x => x.PlayerID == _network.PlayerId));
        }
    }
}