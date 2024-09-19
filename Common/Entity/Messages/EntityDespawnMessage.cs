using Common.Network;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Entity.Messages;

public sealed class EntityDespawnMessage : INetworkMessage
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;

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