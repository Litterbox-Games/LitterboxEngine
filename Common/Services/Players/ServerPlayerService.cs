using Common.Services.Events;
using Common.Services.Logging;
using Common.Services.Players.Events;

namespace Common.Services.Players;

public sealed class ServerPlayerService : IPlayerService
{
    public ulong PlayerId { get; }
    public IEnumerable<NetworkPlayer> Players => _players;

    private readonly List<NetworkPlayer> _players = [];
    
    private readonly EventService _eventService;
    private readonly ILoggingService _logger;

    public ServerPlayerService(EventService eventService, ILoggingService logger)
    {
        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        
        _eventService = eventService;
        _logger = logger;
        
        eventService.Handle<PlayerConnectEvent>(OnPlayerConnect);
        eventService.Handle<PlayerDisconnectEvent>(OnPlayerDisconnectMessage);
    }

    public void SpawnServerPlayer()
    {
        var player = new NetworkPlayer(PlayerId, $"Player {PlayerId}");
            
        _eventService.Emit(new PlayerConnectEvent 
        { 
            NetworkPlayer = player, 
            Receivers = serverPlayer => serverPlayer != player 
        });
    }
    
    private void OnPlayerConnect(PlayerConnectEvent e)
    {
        
        _logger.Information($"{e.NetworkPlayer.PlayerName} has connected!");
        
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
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}