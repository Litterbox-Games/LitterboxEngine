using Common.DI;
using Common.Network;
using Common.Player;
using Common.Player.Messages;
using ClientPlayer = Common.Player.Player;

namespace Client.Player;

public class ClientPlayerService : IPlayerService
{
    private readonly List<Common.Player.Player> _players = new();

    private IHost _host;
    private INetworkService _network;

    public IEnumerable<Common.Player.Player> Players => _players;

    public ClientPlayerService(IHost host, INetworkService networkService)
    {
        _host = host;
        _network = networkService;

        _network.RegisterMessageHandle<PlayerConnectMessage>(OnPlayerConnectMessage);
        _network.RegisterMessageHandle<PlayerDisconnectMessage>(OnPlayerDisconnectMessage);
        _network.RegisterMessageHandle<PlayerListSyncMessage>(OnPlayerListSyncMessage);
    }

    private void OnPlayerConnectMessage(INetworkMessage message, ClientPlayer? _)
    {
        var castedMessage = (PlayerConnectMessage) message;
        _players.Add(castedMessage.Player!);
    }

    private void OnPlayerDisconnectMessage(INetworkMessage message, ClientPlayer? _)
    {
        var castedMessage = (PlayerDisconnectMessage) message;

        _players.Remove(_players.First(x => x.PlayerID == castedMessage.PlayerId));
    }

    private void OnPlayerListSyncMessage(INetworkMessage message, ClientPlayer? _)
    {
        var castedMessage = (PlayerListSyncMessage) message;

        _players.Clear();
        _players.AddRange(castedMessage.Players);
    }
}