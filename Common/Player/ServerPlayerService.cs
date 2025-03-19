using Common.DI;
using Common.Host;
using Common.Network;
using Common.Player.Messages;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Player;

public sealed class ServerPlayerService : IPlayerService
{
    public ulong PlayerId { get; }
    public IEnumerable<NetworkPlayer> Players => _network.Players;

    private readonly IServerNetworkService _network;
    private readonly IContainer _container;

    public ServerPlayerService(IContainer container, IServerNetworkService networkService)
    {
        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        
        _container = container;
        _network = networkService;
        
        _network.EventOnPlayerConnect += OnPlayerConnect;
        _network.EventOnPlayerDisconnect += OnPlayerDisconnect;
    }

    private void OnPlayerConnect(ServerPlayer player)
    {
        var syncMessage = new PlayerListSyncMessage();

        foreach (var p in Players)
        {
            syncMessage.Players.Add(p);
        }

        _network.SendToPlayer(syncMessage, player);

        if (Players.Count() < 2)
        {
            return;
        }

        var connectMessage = new PlayerConnectMessage
        {
            NetworkPlayer = player
        };

        foreach (var p in Players)
        {
            if (p.PlayerID != player.PlayerID && p.PlayerID != PlayerId)
            {
                _network.SendToPlayer(connectMessage, (ServerPlayer) p);
            }
        }
    }

    private void OnPlayerDisconnect(ServerPlayer player)
    {
        if (!Players.Any() || Players.Count() == 2 && _container.GameMode == EGameMode.Host)
            return;

        var disconnectMessage = new PlayerDisconnectMessage
        {
            PlayerId = player.PlayerID
        };

        foreach (var p in Players)
        {
            if (p.PlayerID != player.PlayerID)
            {
                _network.SendToPlayer(disconnectMessage, (ServerPlayer) p);
            }
        }
    }
}