using Common.Network;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Player.Messages;

public sealed class PlayerDisconnectMessage : INetworkMessage
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;

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