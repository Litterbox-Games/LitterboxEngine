using Common.Core.Attributes;
using Common.Services.Events;
using Common.Services.Players;
using Common.Services.Players.Events;

namespace Client.Services.Players;

[Game(EMode.Client)]
[As<IPlayerService>]
public class ClientPlayerService : IPlayerService
{
    public ulong PlayerId { get; }

    public IEnumerable<NetworkPlayer> Players => _players;

    private readonly List<NetworkPlayer> _players = [];
    private readonly EventService _eventService;
    
    public ClientPlayerService(EventService eventService)
    {
        _eventService = eventService;
        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        
        _eventService.Handle<PlayerConnectEvent>(OnPlayerConnectMessage);
        _eventService.Handle<PlayerDisconnectEvent>(OnPlayerDisconnectMessage);
        _eventService.Handle<PlayerListSyncEvent>(OnPlayerListSyncMessage);
    }

    private void OnPlayerConnectMessage(PlayerConnectEvent e)
    {
        _players.Add(e.NetworkPlayer);
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
    
    public void Dispose()
    {
        _eventService.Unhandle<PlayerConnectEvent>(OnPlayerConnectMessage);
        _eventService.Unhandle<PlayerDisconnectEvent>(OnPlayerDisconnectMessage);
        _eventService.Unhandle<PlayerListSyncEvent>(OnPlayerListSyncMessage);
        
        GC.SuppressFinalize(this);
    }
}