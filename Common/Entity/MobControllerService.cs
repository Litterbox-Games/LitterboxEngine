using System.Numerics;
using Common.DI;

namespace Common.Entity;

public class MobControllerService : ITickableService
{
    private readonly ServerEntityService _entityService;

    private readonly List<MobEntity> _entities = new();

    private readonly Random _random = new();

    public MobControllerService(IEntityService entityService)
    {
        if (entityService is not ServerEntityService service)
            throw new InvalidOperationException("This service is not valid without ServerEntityService");

        _entityService = service;

        _entityService.EventOnEntitySpawn += OnEntitySpawn;
        _entityService.EventOnEntityDespawn += OnEntityDespawn;
    }

    public void SpawnMobEntity(Vector2 position)
    {
        var signX = _random.Next() > int.MaxValue / 2 ? -1 : 1;
        var signY = _random.Next() > int.MaxValue / 2 ? -1 : 1;
        var entity = new MobEntity
        {
            Position = position,
            OwnerId = 0,
            EntityId = (ulong) _random.Next(),
            Direction = Vector2.Normalize(new Vector2(signX * _random.Next(), signY * _random.Next())),
            LastChangedDirections = DateTime.Now
        };
        
        _entityService.SpawnEntity(entity);
    }

    public void Update(float deltaTime)
    {
        const float movementSpeed = 5f;

        _entities.ForEach(x =>
        {
            // Change direction randomly if we haven't changed directions in the last 3-7 seconds
            if (DateTime.Now - x.LastChangedDirections > new TimeSpan(0, 0, 0, _random.Next() % 5 + 3))
            {
                var signX = _random.Next() > int.MaxValue / 2 ? -1 : 1;
                var signY = _random.Next() > int.MaxValue / 2 ? -1 : 1;
                x.Direction = Vector2.Normalize(new Vector2(signX * _random.Next(), signY * _random.Next()));
                x.LastChangedDirections = DateTime.Now;
            }
            
            x.Position += x.Direction * deltaTime * movementSpeed;
        });
    }

    public void Draw() { }

    private void OnEntitySpawn(GameEntity entity)
    {
        if (entity is not MobEntity mobEntity)
            return;
        
        _entities.Add(mobEntity);
    }

    private void OnEntityDespawn(GameEntity entity)
    {
        if (entity is not MobEntity mobEntity)
            return;
        
        _entities.Remove(mobEntity);
    }
}