using Common.Mathematics;
using Common.Network;
using Lidgren.Network;

namespace Common.World.Messages;

public sealed class BlockUpdateMessage : INetworkMessage
{
    NetDeliveryMethod INetworkMessage.NetworkChannel => NetDeliveryMethod.ReliableOrdered;

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