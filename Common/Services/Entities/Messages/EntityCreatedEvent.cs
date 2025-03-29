using Arch.Core;
using Common.Services.Events;

namespace Common.Services.Entities.Messages;

public struct EntityCreatedEvent: IEvent
{
    public Entity Entity;
}