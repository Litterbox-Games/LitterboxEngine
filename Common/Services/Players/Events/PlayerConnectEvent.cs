using Common.Services.Network.Events;
using Lidgren.Network;

namespace Common.Services.Players.Events;

public sealed class PlayerConnectEvent : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;
    public NetworkPlayer? Sender { get; set; }
    public Predicate<NetworkPlayer>? Receivers { get; set; }
    
    public required NetworkPlayer NetworkPlayer;

    public void Serialize(NetOutgoingMessage writer)
    {
        if (NetworkPlayer == null)
        {
            throw new InvalidOperationException("Player cannot be null");
        }

        writer.Write(NetworkPlayer.PlayerId);
        writer.Write(NetworkPlayer.PlayerName);
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        var playerId = reader.ReadUInt64();
        var playerName = reader.ReadString();

        NetworkPlayer = new NetworkPlayer(playerId, playerName);
    }
}