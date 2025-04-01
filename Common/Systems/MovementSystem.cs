using System.Numerics;
using Arch.Core;
using Common.Components;
using Common.Core;
using Common.Services.Entities;
using Common.Services.Entities.Events;
using Common.Services.Events;
using Common.Services.Players;

namespace Common.Systems;

public class MovementSystem: ISystem, IUpdatable
{
    private readonly QueryDescription _movable = new QueryDescription().WithAll<Networked, Position>();

    private readonly EventService _eventService;
    private readonly IEntityService _entityService;
    private readonly IPlayerService _playerService;
    
    public MovementSystem(IEntityService entityService, IPlayerService playerService, EventService eventService)
    {
        _eventService = eventService;
        _entityService = entityService;
        _playerService = playerService;
        
        _eventService.Handle<EntityMoveEvent>(OnEntityMoveMessage);
    }
    
    public void Update(float deltaTime)
    {
        // TODO: estimate needed capacity before adding entities?
        var moveMessage = new EntityMoveEvent();
        var now = DateTime.Now;
        var renderTime = now - new TimeSpan(0, 0, 0, 0, 100);
        
        _entityService.Entities.Query(in _movable, ( 
            ref Networked network, 
            ref Position position
        ) => {
            if (position.Current == position.LastSent) return;

            if (network.OwnerId == _playerService.PlayerId && (now - position.LastUpdate).TotalMilliseconds >= 50)
            {
                moveMessage.Entities.Add(new EntityMovement
                {
                    EntityId = network.NetworkId,
                    NewPosition = position.Current
                });

                position.LastSent = position.Current;
                position.LastUpdate = now;    
            } 
            else if (position.Queued.Count >= 2)
            {
                while (position.Queued.Count > 2 && renderTime > position.Queued.ToArray()[1].TimeStamp)
                {
                    position.Queued.Dequeue();
                }
                
                var firstMovement = position.Queued.ToArray()[0];
                var secondMovement = position.Queued.ToArray()[1];
                
                var interpolationFactor = (renderTime - firstMovement.TimeStamp).TotalMilliseconds /
                                          (secondMovement.TimeStamp -
                                           firstMovement.TimeStamp).TotalMilliseconds;

                interpolationFactor = interpolationFactor > 1 ? 1 : interpolationFactor;
                position.Current = Vector2.Lerp(firstMovement.Position, secondMovement.Position, (float) interpolationFactor);
            }
        });
        
        if (moveMessage.Entities.Count > 0)
            _eventService.Outgoing(moveMessage);
    }
    
    private void OnEntityMoveMessage(EntityMoveEvent e)
    {
        var now = DateTime.Now;

        // Iterate Entities once because message.Entities.Length < Entities.Length (typically)
        _entityService.Entities.Query(in _movable, (
            ref Networked network, 
            ref Position position
        ) =>
        {
            if (network.NetworkId == _playerService.PlayerId) return;
            
            var entityId = network.NetworkId;
            var movement = e.Entities.FirstOrDefault(x => x.EntityId == entityId);   
            
            if (movement == null) return;
            
            position.Queued.Enqueue(new QueuedMovement(movement.NewPosition, now));
        
            // TODO: Shouldn't need to do this unless Entity changes owners (car maybe?)
            position.LastSent = movement.NewPosition;
        });

        // Forward this packet to all players but sender
        e.Receivers = networkPlayer => networkPlayer != e.Sender; 
        _eventService.Outgoing(e);
    }
}

