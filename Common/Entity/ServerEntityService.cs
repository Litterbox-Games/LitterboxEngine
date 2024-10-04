using System.Numerics;
using Common.Entity.Messages;
using Common.Network;
using Common.Player;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Entity;

public class ServerEntityService : AbstractEntityService
{
    public override List<GameEntity> Entities { get; }
    
    public override event Action<GameEntity>? EventOnEntitySpawn;
    public override event Action<GameEntity>? EventOnEntityDespawn;
    public override event Action<GameEntity>? EventOnEntityMove;

    private readonly ServerNetworkService _network;
    
    public ServerEntityService(ServerNetworkService network)
    {
        Entities = new List<GameEntity>();
        _network = network;
        
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

        foreach (var entity in Entities.Where(x => x.Position != x.LastSentPosition && (now - x.LastUpdateTime).TotalMilliseconds > 50))
        {
            if (entity.OwnerId == _network.PlayerId || entity.OwnerId == 0)
            {
                moveMessage.Entities.Add(new EntityMovement
                {
                    EntityId = entity.EntityId,
                    NewPosition = entity.Position
                });

                entity.LastSentPosition = entity.Position;
                entity.LastUpdateTime = now;
            }
            else if (entity.OwnerId != _network.PlayerId)
            {
                if (entity.QueuedMovements.Count <= 1)
                    return;
                
                while (entity.QueuedMovements.Count > 2 && renderTime > entity.QueuedMovements.ToArray()[1].TimeStamp)
                {
                    entity.QueuedMovements.Dequeue();
                }

                var firstMovement = entity.QueuedMovements.ElementAt(0);
                var secondMovement = entity.QueuedMovements.ElementAt(1);

                var interpolationFactor = (renderTime - firstMovement.TimeStamp).TotalMilliseconds /
                                          (secondMovement.TimeStamp -
                                           firstMovement.TimeStamp).TotalMilliseconds;

                interpolationFactor = interpolationFactor > 1 ? 1 : interpolationFactor;
                entity.Position = Vector2.Lerp(firstMovement.Position, secondMovement.Position, (float) interpolationFactor);
            }
        }
        
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
            var entity = Entities.FirstOrDefault(x => x.EntityId == entityMovement.EntityId);

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
        Entities.Add(entity);

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
        Entities.Remove(entity);

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
        
        foreach (var entityToSend in Entities.Where(entityToSend => entityToSend.EntityId != player.PlayerID))
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
        var entity = Entities.First(x => x.EntityId == player.PlayerID);

        Entities.Remove(entity);

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