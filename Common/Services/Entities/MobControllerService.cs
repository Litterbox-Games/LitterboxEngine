using System.Numerics;
using Arch.Core;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Services.Players;

namespace Common.Services.Entities;

[Game(EMode.Host)]
public class MobControllerService : IService, IUpdatable
{
    private readonly QueryDescription _mobs = new QueryDescription().WithAll<Mob, Position, Velocity>();
    
    private readonly ServerEntityService _entityService;
    private readonly IPlayerService _playerService;
    
    private readonly Random _random = new();
    
    private const float MovementSpeed = 5f;

    public MobControllerService(ServerEntityService entityService, IPlayerService playerService)
    {
        _entityService = entityService;
        _playerService = playerService;
    }

    public void SpawnMobEntity(Vector2 position)
    {
        var signX = _random.Next() > int.MaxValue / 2 ? -1 : 1;
        var signY = _random.Next() > int.MaxValue / 2 ? -1 : 1;
        var velocity = Vector2.Normalize(new Vector2(signX * _random.Next(), signY * _random.Next())) * MovementSpeed;
        
        var entity = _entityService.Entities.Create(
            new Networked { OwnerId = _playerService.PlayerId, NetworkId = (ulong) _random.Next(), EntityType = 1 },
            new Mob(), 
            new Position(position),
            new Velocity(velocity.X, velocity.Y));
        
        _entityService.SpawnEntity(entity);
    }

    public void Update(float deltaTime)
    {
        _entityService.Entities.Query(_mobs, (
            ref Mob mob,
            ref Velocity velocity
        ) =>
        {
            // Change direction randomly if we haven't changed directions in the last 3-7 seconds
            if (DateTime.Now - mob.LastChangedDirections <= new TimeSpan(0, 0, 0, _random.Next() % 5 + 3)) return;
            
            var signX = _random.Next() > int.MaxValue / 2 ? -1 : 1;
            var signY = _random.Next() > int.MaxValue / 2 ? -1 : 1;
            velocity = Vector2.Normalize(new Vector2(signX * _random.Next(), signY * _random.Next())) * MovementSpeed;
            mob.LastChangedDirections = DateTime.Now;
        });
        
        _entityService.Entities.Query(_mobs, (
            ref Position position,
            ref Velocity velocity
        ) => {
            position.Current += velocity.ToVector2() * deltaTime;
        });
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}