using Arch.Core;
using Arch.Core.Extensions;
using Common.Components;

namespace Common.Archetypes;

public interface IArchetype
{
    // TODO: enforce a query for this Archetype?
    public static abstract void Create(Entity entity);
}

public sealed class PlayerArchetype : IArchetype
{
    public static void Create(Entity entity)
    {
        // TODO: use a hashing system like we do for messages for creating the correct entity
        // TODO: Will be added by the creator of the Entity?
        // entity.Add(new Networked { OwnerId = e.EntityOwner, NetworkId = e.EntityId, EntityType = e.EntityType });
        // entity.Add(new Position(e.EntityPosition)); 
        
        entity.Add<Velocity>();
        entity.Add<Player>();

        // TODO: would force all components to live in Common? - this is probably fine
        // if (entity.Has<HostOwned>())
        // {
        //     entity.Add<PlayerControls>();
        //     entity.Add<CameraFollow>();    
        // }
    }
}