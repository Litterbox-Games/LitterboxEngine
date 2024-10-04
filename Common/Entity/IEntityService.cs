using System.Numerics;
using Common.DI;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Entity;

public interface IEntityService : ITickableService
{
    List<GameEntity> Entities { get; }

    event Action<GameEntity>? EventOnEntitySpawn;
    event Action<GameEntity>? EventOnEntityDespawn;
    event Action<GameEntity>? EventOnEntityMove;
}