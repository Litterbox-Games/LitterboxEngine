using System.Numerics;
using Common.DI;

namespace Common.Entity;

public class MobControllerService : ITickableService
{
    private ServerEntityService _entityService;

    private readonly List<MobEntity> _entities = [];

    private Random _random = new();
    
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
        var entity = new MobEntity
        {
            Position = position,
            OwnerId = 0,
            EntityId = (ulong) _random.Next()
        };
        
        _entityService.SpawnEntity(entity);
    }

    private float totalTime = 0f;
    
    public void Update(float deltaTime)
    {
        totalTime += deltaTime;
        
        _entities.ForEach(x =>
        {
            x.Position = new Vector2(MathF.Sin(totalTime) * 2, x.Position.Y);
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