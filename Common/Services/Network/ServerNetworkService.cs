using Common.Core;
using Common.Host;
using Common.Services.Events;
using Common.Services.Logging;
using Common.Services.Network.Events;
using Common.Services.Players;
using Common.Services.Players.Events;
using Lidgren.Network;

namespace Common.Services.Network;

public sealed class ServerNetworkService(IContainer container, ILoggingService logger, EventService eventService): NetworkService(logger, eventService)
{
    private NetServer? _server;
    protected override NetPeer NetPeer => _server!;

    private readonly ILoggingService _logger = logger;
    private readonly EventService _eventService = eventService;
    
    private readonly Dictionary<NetworkPlayer, NetConnection> _connections = new();
    
    protected override void OnOutgoing(OutgoingEvent e)
    {
        IEnumerable<NetworkPlayer> players = _connections.Keys;
        if (e.NetworkEvent.Receivers != null)
            players = players.Where(p => e.NetworkEvent.Receivers(p));
        
        var connections = players.Where(p => _connections.ContainsKey(p)).Select(p => _connections[p]).ToList();

        if (connections.Count == 0) return;
        
        SendMessage(connections, e.NetworkEvent);
    }
    
    public override void Update(float deltaTime)
    {
        if (_server == null)
            return;
        
        if (_server.Status != NetPeerStatus.Running)
            return;
        
        while (_server.ReadMessage() is { } message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    var status = (NetConnectionStatus) message.ReadByte();
                    var reason = message.ReadString();

                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (status)
                    {
                        case NetConnectionStatus.Connected:
                            OnConnect(message.SenderConnection);
                            continue;
                        case NetConnectionStatus.Disconnected:
                            OnDisconnect(message.SenderConnection);
                            continue;
                    }

                    OnStatusChange(status, reason);

                    break;
                case NetIncomingMessageType.ConnectionApproval:
                    var approve = OnConnectionRequest(message); 
                    
                    if (approve)
                        message.SenderConnection.Approve();
                    else
                        message.SenderConnection.Deny();

                    break;

                case NetIncomingMessageType.Data:
                    var e = OnData(message);
                    if (e == null) break;
                    e.Sender = _connections.FirstOrDefault(x => x.Value.RemoteUniqueIdentifier == message.SenderConnection.RemoteUniqueIdentifier).Key;
                    _eventService.Incoming(e);
                    break;
            }
        }
    }

    public void Listen(ushort port)
    {
        if (_server != null) throw new InvalidOperationException("Server is already listening and must be destroyed.");
        
        var config = new NetPeerConfiguration("Ages of Automation") { 
            Port = port,
            PingInterval = 1f,
            ConnectionTimeout = 5f
        };

        config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
        
        _server = new NetServer(config);
        _server.Start();
        
        _logger.Information("Server is now listening on port 7777.");
    }

    public void StopListening()
    {
        if (_server == null) return;

        _server.Shutdown("Server has been shutdown by host.");

        _server = null;
    }
    
    private bool OnConnectionRequest(NetIncomingMessage message)
    {
        if (container.GameMode == EGameMode.SinglePlayer)
            return false;
        
        try
        {
            var player = new NetworkPlayer(message.ReadUInt64(), message.ReadString());
            _connections.Add(player, message.SenderConnection);
        }
        catch (Exception e)
        {
            _logger.Error("Encountered an error during a connection request!");
            _logger.Error(e.Message);
            if (e.StackTrace != null) _logger.Error(e.StackTrace);
            
            return false;
        }

        return true;
    }

    private void OnConnect(NetConnection conn)
    {
        var player = _connections.FirstOrDefault(x => x.Value.RemoteUniqueIdentifier == conn.RemoteUniqueIdentifier).Key;

        if (player == null)
        {
            conn.Disconnect("Authentication failed or wasn't performed.");
            return;
        }
        
        _eventService.Emit(new PlayerConnectEvent 
        { 
            NetworkPlayer = player, 
            Receivers = serverPlayer => serverPlayer != player 
        });
    }

    private void OnDisconnect(NetConnection conn)
    {
        var player = _connections.FirstOrDefault(x => x.Value.RemoteUniqueIdentifier == conn.RemoteUniqueIdentifier).Key;
        
        if (player == null)
            return;

        _connections.Remove(player);

        _logger.Information($"{player.PlayerName} has disconnected!");
        
        _eventService.Emit(new PlayerDisconnectEvent
        {
            PlayerId = player.PlayerId,
            Receivers = serverPlayer => serverPlayer != player
        });
    }
    
    private void OnStatusChange(NetConnectionStatus newStatus, string reason)
    {
        _logger.Debug($"Server status changed to {newStatus} for reason: {reason}!");
    }
}