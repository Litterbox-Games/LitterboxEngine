using System.Numerics;
using Arch.Core;
using Common.Components;
using Common.Core;
using Common.Services.Entities;

namespace Common.Systems;

public class MobControllerSystem : ISystem, IUpdatable
{
    private readonly QueryDescription _mobs = new QueryDescription().WithAll<Mob, Position, Velocity>();
    
    private readonly ServerEntityService _entityService;

    private readonly Random _random = new();

    public MobControllerSystem(ServerEntityService entityService)
    {
        _entityService = entityService;
    }

    public void SpawnMobEntity(Vector2 position)
    {
        var signX = _random.Next() > int.MaxValue / 2 ? -1 : 1;
        var signY = _random.Next() > int.MaxValue / 2 ? -1 : 1;
        var velocity = Vector2.Normalize(new Vector2(signX * _random.Next(), signY * _random.Next()));
        
        var entity = _entityService.Entities.Create(
            new Networked { OwnerId = 0, NetworkId = (ulong) _random.Next(), EntityType = 1 },
            new Mob(), 
            new Position(position),
            new Velocity(velocity.X, velocity.Y) );
        
        _entityService.SpawnEntity(entity);
    }

    public void Update(float deltaTime)
    {
        const float movementSpeed = 5f;

        _entityService.Entities.Query(_mobs, (
            ref Mob mob,
            ref Velocity velocity
        ) =>
        {
            // Change direction randomly if we haven't changed directions in the last 3-7 seconds
            if (DateTime.Now - mob.LastChangedDirections <= new TimeSpan(0, 0, 0, _random.Next() % 5 + 3)) return;
            
            var signX = _random.Next() > int.MaxValue / 2 ? -1 : 1;
            var signY = _random.Next() > int.MaxValue / 2 ? -1 : 1;
            velocity = Vector2.Normalize(new Vector2(signX * _random.Next(), signY * _random.Next())) * movementSpeed;
            mob.LastChangedDirections = DateTime.Now;
        });
        
        _entityService.Entities.Query(_mobs, (
            ref Position position,
            ref Velocity velocity
        ) => {
            position.Current += velocity.ToVector2() * deltaTime;
        });
    }
}