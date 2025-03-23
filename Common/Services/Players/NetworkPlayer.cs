using Lidgren.Network;

namespace Common.Players;

public class NetworkPlayer(ulong id, string name)
{
    public ulong PlayerId { get; } = id;
    public string PlayerName { get; } = name;

}

public sealed class ServerPlayer(ulong id, string name, NetConnection? conn) : NetworkPlayer(id, name)
{
    public NetConnection? PlayerConnection { get; } = conn;
}