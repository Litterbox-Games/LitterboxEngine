using Common.Mathematics;
using Common.Network;
using Lidgren.Network;

namespace Common.World.Messages;

public sealed class ChunkRequestMessage : INetworkMessage
{
    NetDeliveryMethod INetworkMessage.NetworkChannel => NetDeliveryMethod.ReliableOrdered;

    public EChunkRequest RequestType;
    public Vector2i[]? Chunks;

    public void Serialize(NetOutgoingMessage writer)
    {
        if (Chunks == null || Chunks.Length == 0)
            throw new InvalidOperationException("Cannot send a chunk request message with no request coordinates.");
        
        writer.Write((byte) RequestType);
        writer.Write(Chunks.Length);

        foreach (var chunk in Chunks)
        {
            writer.Write(chunk.X);
            writer.Write(chunk.Y);
        }
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        RequestType = (EChunkRequest) reader.ReadByte();

        var length = reader.ReadInt32();

        Chunks = new Vector2i[length];

        for (var i = 0; i < length; i++)
        {
            Chunks[i] = new Vector2i(reader.ReadInt32(), reader.ReadInt32());
        }
    }
}