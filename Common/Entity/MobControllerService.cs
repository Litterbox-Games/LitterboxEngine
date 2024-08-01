using System.Numerics;
using Common.DI;

namespace Common.Entity;

public class MobControllerService : ITickableService
{
    private ServerEntityService _entityService;

    private readonly List<MobEntity> _entities = [];
    
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
            EntityId = (ulong) new Random(DateTime.Now.Millisecond).Next()
        };
        
        _entityService.SpawnEntity(entity);
    }

    private bool movingPosX = true;
    
    public void Update(float deltaTime)
    {
        _entities.ForEach(x =>
        {
            movingPosX = x.Position.X switch
            {
                > 5 => false,
                < -5 => true,
                _ => movingPosX
            };

            if (movingPosX)
                x.Position += new Vector2(deltaTime * 5, 0);
            else
                x.Position -= new Vector2(deltaTime * 5, 0);
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