using System.Numerics;
using Common.Services.Network.Events;
using Common.Services.Players;
using Lidgren.Network;

namespace Common.Services.Entities.Events;

public record EntityMovement
{
    public ulong EntityId;
    public Vector2 NewPosition;
    // Default to Interpolate
    public ESyncMode SyncMode = ESyncMode.Interpolate;
}

public struct EntityMoveEvent() : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.UnreliableSequenced;
    public NetworkPlayer? Sender { get; set; } = null;
    public Predicate<NetworkPlayer>? Receivers { get; set; } = null;

    public List<EntityMovement> Entities = [];

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
        Entities = new List<EntityMovement>(length);

        for (var i = 0; i < length; i++)
        {
            Entities.Add(new EntityMovement
            {
                EntityId = reader.ReadUInt64(),
                NewPosition = new Vector2(reader.ReadFloat(), reader.ReadFloat()),
                SyncMode = (ESyncMode) reader.ReadByte()
            });
        }
    }
}

public enum ESyncMode : byte
{
    Interpolate = 0,
    Teleport = 1
}