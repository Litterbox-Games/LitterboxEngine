using Common.Mathematics;
using Common.Services.Network.Events;
using Common.Services.Players;
using Lidgren.Network;

namespace Common.Services.World.Events;

public struct BlockUpdateEvent : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableOrdered;
    public NetworkPlayer? Sender { get; set; }
    public Predicate<NetworkPlayer>? Receivers { get; set; }
    
    public Vector2i Chunk;
    public Vector2i Position;
    public EBlockType BlockType;
    public ushort Id;

    public void Serialize(NetOutgoingMessage writer)
    {
        writer.Write(Chunk.X);
        writer.Write(Chunk.Y);
        writer.Write(Position.X);
        writer.Write(Position.Y);
        writer.Write((byte)BlockType);
        writer.Write(Id);
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        Chunk = new Vector2i(reader.ReadInt32(), reader.ReadInt32());
        Position = new Vector2i(reader.ReadInt32(), reader.ReadInt32());
        BlockType = (EBlockType)reader.ReadByte();
        Id = reader.ReadUInt16();
    }
}