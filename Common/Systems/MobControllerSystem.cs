using System.Numerics;
using Arch.Core;
using Common.Components;
using Common.DI;
using Common.Entities.Components;
using Common.Systems;

namespace Common.Entities.Systems;

public class MobControllerSystem : ISystem, IUpdatable
{
    private readonly QueryDescription _mobs = new QueryDescription().WithAll<Mob, Position>();
    
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
        
        
        var entity = _entityService.Entities.Create(
            new Networked { OwnerId = 0, NetworkId = (ulong) _random.Next(), EntityType = 1 },
            new Mob { Direction = Vector2.Normalize(new Vector2(signX * _random.Next(), signY * _random.Next()) )}, 
            new Position(position));
        
        _entityService.SpawnEntity(entity);
    }

    public void Update(float deltaTime)
    {
        const float movementSpeed = 5f;

        _entityService.Entities.Query(_mobs, (
            ref Mob mob,
            ref Position position
        ) => {
            // Change direction randomly if we haven't changed directions in the last 3-7 seconds
            if (DateTime.Now - mob.LastChangedDirections > new TimeSpan(0, 0, 0, _random.Next() % 5 + 3))
            {
                var signX = _random.Next() > int.MaxValue / 2 ? -1 : 1;
                var signY = _random.Next() > int.MaxValue / 2 ? -1 : 1;
                mob.Direction = Vector2.Normalize(new Vector2(signX * _random.Next(), signY * _random.Next()));
                mob.LastChangedDirections = DateTime.Now;
            }
            
            position.Current += mob.Direction * deltaTime * movementSpeed;
        });
    }
}