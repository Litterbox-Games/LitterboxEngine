using Arch.Core;
using Common.Services.Events;

namespace Common.Services.Entities.Events;

public struct EntityCreatedEvent: IEvent
{
    public Entity Entity;
}