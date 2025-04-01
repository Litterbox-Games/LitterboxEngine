using Common.Services.Events;

namespace Common.Services.Network.Events;

public struct OutgoingEvent(INetworkEvent networkEvent) : IEvent
{
    public INetworkEvent NetworkEvent = networkEvent;
}