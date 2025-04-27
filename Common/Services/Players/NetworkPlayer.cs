namespace Common.Services.Players;

public class NetworkPlayer(ulong id, string name)
{
    public ulong PlayerId { get; } = id;
    public string PlayerName { get; } = name;

}