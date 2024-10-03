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
        var entity = new MobEntity
        {
            Position = position,
            OwnerId = 0,
            EntityId = (ulong) _random.Next(),
            Direction = Vector2.Normalize(new Vector2(_random.Next(), _random.Next()))
        };
        
        _entityService.SpawnEntity(entity);
    }

    public void Update(float deltaTime)
    {
        const float movementSpeed = 0.5f;

        _entities.ForEach(x =>
        {
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