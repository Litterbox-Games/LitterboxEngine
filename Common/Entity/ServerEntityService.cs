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
        var moveMessage = new EntityMoveMessage();
        
        var now = DateTime.Now;
        var renderTime = now - new TimeSpan(0, 0, 0, 0, 100);
        _entities.ForEach(x =>
        {
            if ((x.OwnerId == _network.PlayerId || x.OwnerId == 0)
                && x.Position != x.LastSentPosition
                && (now - x.LastUpdateTime).TotalMilliseconds > 50)
            {
                moveMessage.Entities.Add(new EntityMovement
                {
                    EntityId = x.EntityId,
                    NewPosition = x.Position
                });

                x.LastSentPosition = x.Position;
                x.LastUpdateTime = now;
            }
            else
            {
                if (x.QueuedMovements.Count <= 1)   
                    return;

                while (x.QueuedMovements.Count > 2 && renderTime > x.QueuedMovements.ElementAt(1).TimeStamp)
                {
                    x.QueuedMovements.Dequeue();
                }

                var firstMovement = x.QueuedMovements.ElementAt(0);
                var secondMovement = x.QueuedMovements.ElementAt(1);
                
                var interpolationFactor = (renderTime - firstMovement.TimeStamp).TotalMilliseconds /
                                          (secondMovement.TimeStamp - firstMovement.TimeStamp).TotalMilliseconds;

                interpolationFactor = interpolationFactor <= 1 ? interpolationFactor : 1;
                x.Position = Vector2.Lerp(firstMovement.Position, secondMovement.Position, (float) interpolationFactor);
            }
        });
        
        if (moveMessage.Entities.Count > 0)
            _network.SendToAllPlayers(moveMessage);
    }

    /// <inheritdoc />
    public override void Draw() { }

    private void OnEntityMoveMessage(INetworkMessage message, NetworkPlayer? player)
    {
        var now = DateTime.Now;
        var castedMessage = (EntityMoveMessage) message;

        castedMessage.Entities.ForEach(entityMovement =>
        {
            var entity = _entities.FirstOrDefault(x => x.EntityId == entityMovement.EntityId);

            if (entity == null)
                return;

            entity.QueuedMovements.Enqueue(new QueuedMovement(entityMovement.NewPosition, now));

            // TODO: Shouldn't need to do this unless Entity changes owners (car maybe?)
            entity.LastSentPosition = entityMovement.NewPosition;

            EventOnEntityMove?.Invoke(entity);    
        });
        
        // Forward this packet to all players
        foreach (var networkPlayer in _network.Players)
        {
            if (networkPlayer != player)
            {
                _network.SendToPlayer(castedMessage, networkPlayer);
            }
        }
    }

    public void SpawnEntity(GameEntity entity)
    {
        _entities.Add(entity);

        var entitySpawnMessage = new EntitySpawnMessage()
        {
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            EntityOwner = entity.OwnerId,
            EntityPosition = entity.Position
        };
        
        _network.SendToAllPlayers(entitySpawnMessage);
        EventOnEntitySpawn?.Invoke(entity);
    }

    public void DespawnEntity(GameEntity entity)
    {
        _entities.Remove(entity);

        var entityDeleteMessage = new EntityDespawnMessage()
        {
            EntityId = entity.EntityId
        };
        
        _network.SendToAllPlayers(entityDeleteMessage);
        
        EventOnEntityDespawn?.Invoke(entity);
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

        SpawnEntity(entity);
        
        foreach (var entityToSend in _entities.Where(entityToSend => entityToSend.EntityId != player.PlayerID))
        {
           var entitySpawnMessage = new EntitySpawnMessage
            {
                EntityId = entityToSend.EntityId,
                EntityOwner = entityToSend.OwnerId,
                EntityType = entityToSend.EntityType,
                EntityPosition = entityToSend.Position,
                EntityData = entityToSend.SerializeEntityData()
            };

            _network.SendToPlayer(entitySpawnMessage, player);
        }
    }

    private void OnPlayerDisconnect(ServerPlayer player)
    {
        var entity = _entities.First(x => x.EntityId == player.PlayerID);

        _entities.Remove(entity);

        // TODO: Allow this to use DespawnEntity Method
        var entityDespawnMessage = new EntityDespawnMessage
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