using Common.Services.Network.Events;
using Lidgren.Network;

namespace Common.Services.Players.Events;

public sealed class PlayerDisconnectEvent : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;
    public NetworkPlayer? Sender { get; set; }
    public Predicate<NetworkPlayer>? Receivers { get; set; }

    public ulong PlayerId;

    public void Serialize(NetOutgoingMessage writer)
    {
        writer.Write(PlayerId);
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        PlayerId = reader.ReadUInt64();
    }
}