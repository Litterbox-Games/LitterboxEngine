using Common.Services.Events;
using Common.Services.Network;
using Lidgren.Network;

namespace Common.Services.Players.Messages;

public sealed class PlayerDisconnectMessage : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;
    public ServerPlayer? Sender { get; set; }
    public Predicate<ServerPlayer>? Receivers { get; set; }

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