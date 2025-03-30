using Common.Services.Network.Events;
using Lidgren.Network;

namespace Common.Services.Players.Events;

public sealed class PlayerListSyncEvent : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;
    public ServerPlayer? Sender { get; set; }
    public Predicate<ServerPlayer>? Receivers { get; set; }

    public readonly List<NetworkPlayer> Players = [];

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