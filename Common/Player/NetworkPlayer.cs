using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Player;

public class NetworkPlayer
{
    public ulong PlayerID { get; }
    public string PlayerName { get; }

    public NetworkPlayer(ulong id, string name)
    {
        PlayerID = id;
        PlayerName = name;
    }
}

public sealed class ServerPlayer : NetworkPlayer
{
    public ServerPlayer(ulong id, string name, NetConnection? conn) : base(id, name)
    {
        PlayerConnection = conn;
    }

    public NetConnection? PlayerConnection { get; }
}