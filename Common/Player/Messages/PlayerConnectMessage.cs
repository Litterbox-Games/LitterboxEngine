using Common.Network;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Player.Messages;

public sealed class PlayerConnectMessage : INetworkMessage
{
    public NetDeliveryMethod NetworkChannel => NetDeliveryMethod.ReliableUnordered;

    public Player Player;

    public void Serialize(NetOutgoingMessage writer)
    {
        if (Player == null)
        {
            throw new InvalidOperationException("Player cannot be null");
        }

        writer.Write(Player.PlayerID);
        writer.Write(Player.PlayerName);
    }

    public void Deserialize(NetIncomingMessage reader)
    {
        var playerId = reader.ReadUInt64();
        var playerName = reader.ReadString();

        Player = new Player(playerId, playerName);
    }
}