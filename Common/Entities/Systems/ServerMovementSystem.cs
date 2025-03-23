using System.Numerics;
using Arch.Core;
using Common.DI;
using Common.Entities.Components;
using Common.Network;
using Common.Players;
using Common.Systems;
using Common.Systems.Messages;

namespace Common.Entities.Systems;

public class ServerMovementSystem: ISystem, IUpdatable
{
    private readonly QueryDescription _movable = new QueryDescription().WithAll<Networked, Position>();

    private readonly IServerNetworkService _network;
    private readonly IEntityService _entityService;
    private readonly IPlayerService _playerService;
    
    public ServerMovementSystem(IEntityService entityService, IServerNetworkService network, IPlayerService playerService)
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

            if (network.OwnerId == _playerService.PlayerId || network.OwnerId == 0 && (now - position.LastUpdate).TotalMilliseconds >= 50)
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
            _network.SendToAllPlayers(moveMessage);
    }
    
    private void OnEntityMoveMessage(INetworkMessage message, NetworkPlayer? player)
    {
        var now = DateTime.Now;
        var castedMessage = (EntityMoveMessage) message;

        // Iterate Entities once because castedMessage.Entities.Length < Entities.Length (typically)
        _entityService.Entities.Query(in _movable, (
            ref Networked network, 
            ref Position position
        ) => {
            var entityId = network.NetworkId;
            var movement = castedMessage.Entities.FirstOrDefault(x => x.EntityId == entityId);   
            
            if (movement == null) return;
            
            position.Queued.Enqueue(new QueuedMovement(movement.NewPosition, now));

            // TODO: Shouldn't need to do this unless Entity changes owners (car maybe?)
            position.LastSent = movement.NewPosition;
        });

        // Forward this packet to all players
        _network.SendToAllPlayers(castedMessage, networkPlayer => networkPlayer != player);
    }
}

