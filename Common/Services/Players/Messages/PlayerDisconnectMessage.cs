using Common.Services.Network;
using Lidgren.Network;

namespace Common.Services.Players.Messages;

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