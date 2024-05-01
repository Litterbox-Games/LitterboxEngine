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

    private ClientNetworkService _network;

    public ClientEntityService(INetworkService network)
    {
        _network = (ClientNetworkService) network;
        
        _network.RegisterMessageHandle<EntitySpawnMessage>(OnEntitySpawnMessage);
        _network.RegisterMessageHandle<EntityDespawnMessage>(OnEntityDespawnMessage);
        _network.RegisterMessageHandle<EntityMoveMessage>(OnEntityMoveMessage);
    }

    public override void Update(float deltaTime)
    {
        _entities.ForEach(x =>
        {
            if ((x.OwnerId == _network.PlayerId || x.OwnerId == 0) &&
                x.Position != x.LastSentPosition &&
                (DateTime.Now - x.LastUpdateTime).TotalMilliseconds > 50)
            {
                var message = new EntityMoveMessage
                {
                    EntityId = x.EntityId,
                    NewPosition = x.Position
                };

                _network.SendToServer(message);

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

    public override void Draw() { }

    private void OnEntitySpawnMessage(INetworkMessage message, Common.Player.Player? _)
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

    private void OnEntityMoveMessage(INetworkMessage message, Common.Player.Player? _)
    {
        var castedMessage = (EntityMoveMessage) message;

        var entity = _entities.FirstOrDefault(x => x.EntityId == castedMessage.EntityId);

        if (entity == null) return;

        entity.QueuedMovements.Enqueue(new QueuedMovement(castedMessage.NewPosition, DateTime.Now));

        // TODO: Shouldn't need to do this unless Entity changes owners (car maybe?)
        entity.LastSentPosition = castedMessage.NewPosition;

        entity.LastUpdateTime = DateTime.Now;

        EventOnEntityMove?.Invoke(entity);
    }

    private void OnEntityDespawnMessage(INetworkMessage message, Common.Player.Player? _)
    {
        var castedMessage = (EntityDespawnMessage) message;

        var entity = _entities.First(x => x.EntityId == castedMessage.EntityId);

        _entities.Remove(entity);

        EventOnEntityDespawn?.Invoke(entity);
    }
}