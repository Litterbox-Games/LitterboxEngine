using System.Numerics;
using Client.Network;
using Common.Entity;
using Common.Entity.Messages;
using Common.Network;
using Common.Player;
using ImGuiNET;

namespace Client.Entity;

public class ClientEntityService : AbstractEntityService
{
    public override List<GameEntity> Entities { get; }

    public override event Action<GameEntity>? EventOnEntitySpawn;
    public override event Action<GameEntity>? EventOnEntityDespawn;
    public override event Action<GameEntity>? EventOnEntityMove;

    private readonly ClientNetworkService _network;

    public ClientEntityService(ClientNetworkService network)
    {
        Entities = new List<GameEntity>();
        
        _network = network;

        _network.RegisterMessageHandle<EntitySpawnMessage>(OnEntitySpawnMessage);
        _network.RegisterMessageHandle<EntityDespawnMessage>(OnEntityDespawnMessage);
        _network.RegisterMessageHandle<EntityMoveMessage>(OnEntityMoveMessage);
    }

    public override void Update(float deltaTime)
    {
        var moveMessage = new EntityMoveMessage();
        var now = DateTime.Now;
        var renderTime = now - new TimeSpan(0, 0, 0, 0, 100);
        
        foreach (var entity in Entities.Where(x => x.Position != x.LastSentPosition))
        {
            if (entity.OwnerId == _network.PlayerId && 
                (now - entity.LastUpdateTime).TotalMilliseconds > 50)
            {
                moveMessage.Entities.Add(new EntityMovement
                {
                    EntityId = entity.EntityId,
                    NewPosition = entity.Position
                });

                entity.LastSentPosition = entity.Position;
                entity.LastUpdateTime = now;
            }
            else
            {
                if (entity.QueuedMovements.Count <= 1)
                    continue;
                
                while (entity.QueuedMovements.Count > 2 && renderTime > entity.QueuedMovements.ElementAt(1).TimeStamp)
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
            _network.SendToServer(moveMessage);
        
    }

    public override void Draw()
    {
        ImGui.Begin("EntityService");
        
        foreach (var entity in Entities)
        {
            ImGui.Text($"{entity.EntityId} {entity.OwnerId} ({entity.Position.X}, {entity.Position.Y})");    
        }
        
        ImGui.End();
        
    }

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

        Entities.Add(entity);

        EventOnEntitySpawn?.Invoke(entity);
    }

    private void OnEntityMoveMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var now = DateTime.Now;
        var castedMessage = (EntityMoveMessage) message;

        foreach (var entityMovement in castedMessage.Entities)
        {
            var entity = Entities.FirstOrDefault(x => x.EntityId == entityMovement.EntityId);

            if (entity == null)
                continue;

            entity.QueuedMovements.Enqueue(new QueuedMovement(entityMovement.NewPosition, now));

            // TODO: Shouldn't need to do this unless Entity changes owners (car maybe?)
            entity.LastSentPosition = entityMovement.NewPosition;

            EventOnEntityMove?.Invoke(entity);
        }
    }

    private void OnEntityDespawnMessage(INetworkMessage message, NetworkPlayer? _)
    {
        var castedMessage = (EntityDespawnMessage) message;

        var entity = Entities.First(x => x.EntityId == castedMessage.EntityId);

        Entities.Remove(entity);

        EventOnEntityDespawn?.Invoke(entity);
    }
}