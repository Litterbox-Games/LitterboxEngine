using Common.Services.Network;
using Lidgren.Network;

namespace Common.Services.Entities.Messages;

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