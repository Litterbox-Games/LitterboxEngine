using Common.Core;
using Common.Host;
using Common.Services.Network;
using Common.Services.Players.Messages;

namespace Common.Services.Players;

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
            if (p.PlayerId != player.PlayerId && p.PlayerId != PlayerId)
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
            PlayerId = player.PlayerId
        };

        foreach (var p in Players)
        {
            if (p.PlayerId != player.PlayerId)
            {
                _network.SendToPlayer(disconnectMessage, (ServerPlayer) p);
            }
        }
    }
}