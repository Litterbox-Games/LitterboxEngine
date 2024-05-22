﻿namespace Common.Entity;

public abstract class AbstractEntityService : IEntityService
{
    public abstract IEnumerable<GameEntity> Entities { get; }
    
    public abstract event Action<GameEntity>? EventOnEntitySpawn;
    public abstract event Action<GameEntity>? EventOnEntityDespawn;
    public abstract event Action<GameEntity>? EventOnEntityMove;

    public abstract void Update(float deltaTime);
    public abstract void Draw();
}