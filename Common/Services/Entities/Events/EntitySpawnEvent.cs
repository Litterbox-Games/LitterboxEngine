using System.Numerics;
using Common.Services.Network.Events;
using Common.Services.Players;
using Lidgren.Network;

namespace Common.Services.Entities.Events;

public struct EntitySpawnEvent() : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;
    public ServerPlayer? Sender { get; set; } = null;

    public Predicate<ServerPlayer>? Receivers { get; set; } = null;

    public ulong EntityId = 0;
    public ulong EntityOwner = 0;
    public ushort EntityType = 0;
    public Vector2 EntityPosition = default;

    // I want to use a Dictionary based system in the future with strings as keys and different possible data types,
    // but this will suffice for now
    public byte[] EntityData = [];

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