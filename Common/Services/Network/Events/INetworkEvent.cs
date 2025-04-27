using Common.Services.Events;
using Common.Services.Players;
using Lidgren.Network;

namespace Common.Services.Network.Events;

public interface INetworkEvent : IEvent
{
    public NetDeliveryMethod NetworkChannel { get; }
    public NetworkPlayer? Sender { get; set; }
    public Predicate<NetworkPlayer>? Receivers { get; set; }

    public void Serialize(NetOutgoingMessage writer);
    public void Deserialize(NetIncomingMessage reader);
}