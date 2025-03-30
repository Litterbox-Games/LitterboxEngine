using Arch.Core;
using Common.Services.Events;

namespace Common.Services.Entities.Events;

public struct EntityDestroyedEvent: IEvent
{
    public Entity Entity;
}