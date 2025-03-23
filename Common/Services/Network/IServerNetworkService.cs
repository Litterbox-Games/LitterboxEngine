using Common.Services.Players;

namespace Common.Services.Network;

public interface IServerNetworkService: INetworkService
{
    public IEnumerable<ServerPlayer> Players { get; }
    
    public event Action? EventOnStartListen;
    public event Action? EventOnStopListen;
    public event Action? EventOnPreStopListen;
    public event Action<ServerPlayer>? EventOnPlayerConnect;
    public event Action<ServerPlayer>? EventOnPlayerDisconnect;
    
    public void Listen(ushort port);
    public void StopListening();
    public void SendToPlayer(INetworkMessage message, ServerPlayer player);
    public void SendToAllPlayers(INetworkMessage message, Predicate<ServerPlayer>? predicate = null);
}