using System.Numerics;
using Common.Network;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Entity.Messages;

public sealed class EntitySpawnMessage : INetworkMessage
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;

    public ulong EntityId;
    public ulong EntityOwner;
    public ushort EntityType;
    public Vector2 EntityPosition;

    // I want to use a Dictionary based system in the future with strings as keys and different possible data types,
    // but this will suffice for now
    public byte[] EntityData = Array.Empty<byte>();

    public void Serialize(NetOutgoingMessage writer)
    {
        writer.Write(EntityId);
        writer.Write(EntityOwner);
        writer.Write(EntityType);
        writer.Write(EntityPosition.X);
        writer.Write(EntityPosition.Y);

        writer.Write(EntityData.Length);

        foreach (var t in EntityData)
        {
            writer.Write(t);
        }
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        EntityId = reader.ReadUInt64();
        EntityOwner = reader.ReadUInt64();
        EntityType = reader.ReadUInt16();
        EntityPosition = new Vector2(reader.ReadFloat(), reader.ReadFloat());

        var dataLength = reader.ReadInt32();
        EntityData = new byte[dataLength];

        for (var i = 0; i < dataLength; i++)
        {
            EntityData[i] = reader.ReadByte();
        }
    }
}