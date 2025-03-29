using Arch.Core;
using Common.Services.Events;

namespace Common.Services.Entities.Messages;

public struct EntityDestroyedEvent: IEvent
{
    public Entity Entity;
}