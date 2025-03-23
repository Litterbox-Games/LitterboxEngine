using Arch.Core;
using Common.DI;

namespace Common.Entities;

public interface IEntityService : IService
{
    public event Action<Entity>? EventOnEntitySpawn;
    public event Action<Entity>? EventOnEntityDespawn;
    
    public Arch.Core.World Entities { get; }
}