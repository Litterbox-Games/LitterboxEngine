using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Network;

public interface INetworkMessage
{
    NetDeliveryMethod NetworkChannel { get; }

    void Serialize(NetOutgoingMessage writer);
    void Deserialize(NetIncomingMessage reader);
}