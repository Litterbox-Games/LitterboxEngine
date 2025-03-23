using Common.Mathematics;
using Common.Network;
using Lidgren.Network;

namespace Common.World.Messages;

public sealed class ChunkDataMessage : INetworkMessage
{
    NetDeliveryMethod INetworkMessage.NetworkChannel => NetDeliveryMethod.ReliableOrdered;

    public Vector2i Position;
    public ushort[]? GroundLayer;
    public ushort[]? ObjectLayer;

    public byte[]? BiomeMap;
    public byte[]? HeatMap;
    public byte[]? MoistureMap;

    public void Serialize(NetOutgoingMessage writer)
    {
        writer.Write(Position.X);
        writer.Write(Position.Y);

        if (GroundLayer == null || ObjectLayer == null || MoistureMap == null || HeatMap == null || BiomeMap == null)
            throw new InvalidOperationException("Cannot send an incomplete chunk data packet.");

        for (var i = 0; i < 256; i++)
        {
            writer.Write(GroundLayer[i]);
            writer.Write(ObjectLayer[i]);
            writer.Write(BiomeMap[i]);
            writer.Write(HeatMap[i]);
            writer.Write(MoistureMap[i]);
        }
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        Position = new Vector2i(reader.ReadInt32(), reader.ReadInt32());

        GroundLayer = new ushort[256];
        ObjectLayer = new ushort[256];

        BiomeMap = new byte[256];
        HeatMap = new byte[256];
        MoistureMap = new byte[256];

        for (var i = 0; i < 256; i++)
        {
            GroundLayer[i] = reader.ReadUInt16();
            ObjectLayer[i] = reader.ReadUInt16();
            BiomeMap[i] = reader.ReadByte();
            HeatMap[i] = reader.ReadByte();
            MoistureMap[i] = reader.ReadByte();
        }
    }
}