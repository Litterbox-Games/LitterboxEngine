using Common.Services.Events;
using Common.Services.Network;
using Lidgren.Network;

namespace Common.Services.Players.Messages;

public sealed class PlayerConnectMessage : INetworkEvent
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;
    public ServerPlayer? Sender { get; set; }
    public Predicate<ServerPlayer>? Receivers { get; set; }
    

    public NetworkPlayer? NetworkPlayer;

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