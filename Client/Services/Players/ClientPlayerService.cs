using Client.Services.Network;
using Common.Services.Events;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.Players.Events;

namespace Client.Services.Players;

public class ClientPlayerService : IPlayerService
{
    public ulong PlayerId { get; }

    public IEnumerable<NetworkPlayer> Players => _players;

    private readonly List<NetworkPlayer> _players = [];
    
    public ClientPlayerService(EventService eventService)
    {
        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        
        eventService.Handle<PlayerConnectEvent>(OnPlayerConnectMessage);
        eventService.Handle<PlayerDisconnectEvent>(OnPlayerDisconnectMessage);
        eventService.Handle<PlayerListSyncEvent>(OnPlayerListSyncMessage);
    }

    private void OnPlayerConnectMessage(PlayerConnectEvent e)
    {
        _players.Add(e.NetworkPlayer!);
    }

    private void OnPlayerDisconnectMessage(PlayerDisconnectEvent e)
    {
        _players.Remove(_players.First(x => x.PlayerId == e.PlayerId));
    }

    private void OnPlayerListSyncMessage(PlayerListSyncEvent e)
    {
        _players.Clear();
        _players.AddRange(e.Players);
    }
}