using System.Numerics;
using Arch.Core;
using Client.Services.Network;
using Common.Components;
using Common.Core;
using Common.Services.Entities;
using Common.Services.Entities.Messages;
using Common.Services.Network;
using Common.Services.Players;

namespace Client.Systems;

public class ClientMovementSystem: ISystem, IUpdatable
{
    private readonly QueryDescription _movable = new QueryDescription().WithAll<Networked, Position>();

    private readonly IClientNetworkService _network;
    private readonly IEntityService _entityService;
    private readonly IPlayerService _playerService;
    
    public ClientMovementSystem(IEntityService entityService, IClientNetworkService network, IPlayerService playerService)
    {
        _entityService = entityService;
        _network = network;
        _playerService = playerService;
        
        _network.RegisterMessageHandle<EntityMoveMessage>(OnEntityMoveMessage);
    }
    
    public void Update(float deltaTime)
    {
        var moveMessage = new EntityMoveMessage();
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
            _network.SendToServer(moveMessage);
    }
    
    private void OnEntityMoveMessage(EntityMoveMessage message, NetworkPlayer? player)
    {
        var now = DateTime.Now;

        // Iterate Entities once because message.Entities.Length < Entities.Length (typically)
        _entityService.Entities.Query(in _movable, (
            ref Networked network, 
            ref Position position
        ) => {
            var entityId = network.NetworkId;
            var movement = message.Entities.FirstOrDefault(x => x.EntityId == entityId);   
            
            if (movement == null) return;
            
            position.Queued.Enqueue(new QueuedMovement(movement.NewPosition, now));

            // TODO: Shouldn't need to do this unless Entity changes owners (car maybe?)
            position.LastSent = movement.NewPosition;
        });
    }
}

