using Common.Services.Network.Events;
using Common.Services.Players;
using Lidgren.Network;

namespace Common.Services.Entities.Events;

public struct EntityDespawnEvent : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;
    public ServerPlayer? Sender { get; set; }
    public Predicate<ServerPlayer>? Receivers { get; set; }
    

    public ulong EntityId;

    public void Serialize(NetOutgoingMessage writer)
    {
        writer.Write(EntityId);
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        EntityId = reader.ReadUInt64();
    }
}