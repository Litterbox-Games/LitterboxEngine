using Common.Network;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Player.Messages;

public sealed class PlayerListSyncMessage : INetworkMessage
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;

    public readonly List<NetworkPlayer> Players = new();

    public void Serialize(NetOutgoingMessage writer)
    {
        writer.Write((ushort) Players.Count);

        foreach (var p in Players)
        {
            writer.Write(p.PlayerID);
            writer.Write(p.PlayerName);
        }
    } 

    public void Deserialize(NetIncomingMessage reader)
    {
        var count = reader.ReadUInt16();

        for (var i = 0; i < count; i++)
        {
            var playerId = reader.ReadUInt64();
            var playerName = reader.ReadString();

            Players.Add(new NetworkPlayer(playerId, playerName));
        }
    }
}