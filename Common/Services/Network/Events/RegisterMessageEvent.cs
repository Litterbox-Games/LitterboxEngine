using Common.Services.Events;

namespace Common.Services.Network.Events;

public struct RegisterMessageEvent(Type eventType) : IEvent
{
    public Type Type = eventType;
}