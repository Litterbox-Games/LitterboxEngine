using System.Numerics;
using Common.Network;
using Lidgren.Network;
using MoreLinq;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Entity.Messages;

public record EntityMovement
{
    public ulong EntityId;
    public Vector2 NewPosition;
    // Default to Interpolate
    public ESyncMode SyncMode = ESyncMode.Interpolate;
}

public sealed class EntityMoveMessage : INetworkMessage
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.UnreliableSequenced;

    public readonly List<EntityMovement> Entities = new();

    public void Serialize(NetOutgoingMessage writer)
    {
        writer.Write(Entities.Count);
        Entities.ForEach(entity =>
        {
            writer.Write(entity.EntityId);
            writer.Write(entity.NewPosition.X);
            writer.Write(entity.NewPosition.Y); 
            writer.Write((byte) entity.SyncMode);
        });
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        var length = reader.ReadInt32();
        Enumerable.Range(0, length).ForEach(_ => Entities.Add( new EntityMovement
        {
            EntityId = reader.ReadUInt64(),
            NewPosition = new Vector2(reader.ReadFloat(), reader.ReadFloat()),
            SyncMode = (ESyncMode) reader.ReadByte()
        }));
    }
}

public enum ESyncMode : byte
{
    Interpolate = 0,
    Teleport = 1
}