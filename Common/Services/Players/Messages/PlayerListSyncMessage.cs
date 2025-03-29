using Common.Services.Events;
using Common.Services.Network;
using Lidgren.Network;

namespace Common.Services.Players.Messages;

public sealed class PlayerListSyncMessage : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;
    public ServerPlayer? Sender { get; set; }
    public Predicate<ServerPlayer>? Receivers { get; set; }

    public readonly List<NetworkPlayer> Players = new();

    public void Serialize(NetOutgoingMessage writer)
    {
        writer.Write((ushort) Players.Count);

        foreach (var p in Players)
        {
            writer.Write(p.PlayerId);
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