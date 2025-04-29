using Common.Services.Events;
using Common.Services.Players.Events;

namespace Common.Services.Players;

public sealed class ServerPlayerService : IPlayerService
{
    public ulong PlayerId { get; }
    public IEnumerable<NetworkPlayer> Players => _players;

    private readonly List<NetworkPlayer> _players = [];
    
    private readonly EventService _eventService;

    public ServerPlayerService(EventService eventService)
    {
        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        
        _eventService = eventService;
        
        eventService.Handle<PlayerConnectEvent>(OnPlayerConnect);
        eventService.Handle<PlayerDisconnectEvent>(OnPlayerDisconnectMessage);
    }

    private void OnPlayerConnect(PlayerConnectEvent e)
    {
        var syncMessage = new PlayerListSyncEvent();

        foreach (var p in Players)
        {
            syncMessage.Players.Add(p);
        }

        syncMessage.Receivers = serverPlayer => serverPlayer == e.NetworkPlayer; 
        _eventService.Emit(syncMessage);
        
        _players.Add(e.NetworkPlayer);
    }
    
    private void OnPlayerDisconnectMessage(PlayerDisconnectEvent e)
    {
        _players.Remove(_players.First(x => x.PlayerId == e.PlayerId));
    }
}