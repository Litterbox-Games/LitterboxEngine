using System.Numerics;
using Common.Network;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Entity.Messages;

public sealed class EntityMoveMessage : INetworkMessage
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.UnreliableSequenced;

    public ulong EntityId;
    public Vector2 NewPosition;

    // Default to Interpolate
    public ESyncMode SyncMode = ESyncMode.Interpolate;

    public void Serialize(NetOutgoingMessage writer)
    {
        writer.Write(EntityId);
        writer.Write(NewPosition.X);
        writer.Write(NewPosition.Y);
        writer.Write((byte) SyncMode);
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        EntityId = reader.ReadUInt64();
        NewPosition = new Vector2(reader.ReadFloat(), reader.ReadFloat());
        SyncMode = (ESyncMode) reader.ReadByte();
    }
}

public enum ESyncMode : byte
{
    Interpolate = 0,
    Teleport = 1
}