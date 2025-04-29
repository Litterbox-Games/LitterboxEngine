using Common.Mathematics;
using Common.Services.Network.Events;
using Common.Services.Players;
using Lidgren.Network;

namespace Common.Services.World.Events;

public struct ChunkRequestEvent : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableOrdered;
    public NetworkPlayer? Sender { get; set; }
    public Predicate<NetworkPlayer>? Receivers { get; set; }
    

    public EChunkRequest RequestType;
    public Vector2i[] Chunks;

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