using System.Numerics;
using Client.Network;
using Common.Entity;
using Common.Entity.Messages;
using Common.Network;
using Common.Player;

namespace Client.Entity;

public class ClientEntityService : AbstractEntityService
{
    public override IEnumerable<GameEntity> Entities => _entities;
    private readonly List<GameEntity> _entities = new();
    
    public override event Action<GameEntity>? EventOnEntitySpawn;
    public override event Action<GameEntity>? EventOnEntityDespawn;
    public override event Action<GameEntity>? EventOnEntityMove;

    private readonly ClientNetworkService _network;

    public ClientEntityService(INetworkService network)
    {
        _network = (ClientNetworkService) network;
        
        _network.RegisterMessageHandle<EntitySpawnMessage>(OnEntitySpawnMessage);
        _network.RegisterMessageHandle<EntityDespawnMessage>(OnEntityDespawnMessage);
        _network.RegisterMessageHandle<EntityMoveMessage>(OnEntityMoveMessage);
    }

    public override void Update(float deltaTime)
    {
        var now = DateTime.Now;
        var renderTime = now - new TimeSpan(0, 0, 0, 0, 100);
        _entities.ForEach(x =>
        {
            if (x.OwnerId == _network.PlayerId &&
                x.Position != x.LastSentPosition &&
                (now - x.LastUpdateTime).TotalMilliseconds > 50)
            {
                var message = new EntityMoveMessage
                {
                    EntityId = x.EntityId,
                    NewPosition = x.Position
                };

                _network.SendToServer(message);

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
    }

    public override void Draw() { }

    private void OnEntitySpawnMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var castedMessage = (EntitySpawnMessage) message;

        // TODO: Lookup entity by `EntityType` and cast to the correct entity.
        var entity = new PlayerEntity
        {
            Position = castedMessage.EntityPosition,
            EntitySystem = this,
            OwnerId = castedMessage.EntityOwner,
            EntityId = castedMessage.EntityId,
            LastSentPosition = castedMessage.EntityPosition,
            LastUpdateTime = DateTime.Now
        };

        entity.DeserializeEntityData(castedMessage.EntityData);

        _entities.Add(entity);

        EventOnEntitySpawn?.Invoke(entity);
    }

    private void OnEntityMoveMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var castedMessage = (EntityMoveMessage) message;

        var entity = _entities.FirstOrDefault(x => x.EntityId == castedMessage.EntityId);

        if (entity == null)
            return;

        entity.QueuedMovements.Enqueue(new QueuedMovement(castedMessage.NewPosition, DateTime.Now));

        // TODO: Shouldn't need to do this unless Entity changes owners (car maybe?)
        entity.LastSentPosition = castedMessage.NewPosition;

        EventOnEntityMove?.Invoke(entity);
    }

    private void OnEntityDespawnMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var castedMessage = (EntityDespawnMessage) message;

        var entity = _entities.First(x => x.EntityId == castedMessage.EntityId);

        _entities.Remove(entity);

        EventOnEntityDespawn?.Invoke(entity);
    }
}