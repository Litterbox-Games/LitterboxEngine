using Common.Core;
using Common.Services.Events;
using Common.Services.Logging;
using Common.Services.Network;
using Common.Services.Network.Events;
using Common.Services.Players;
using Lidgren.Network;

namespace Client.Services.Network;

public class ClientNetworkService: NetworkService
{
    protected override NetPeer NetPeer => _client;

    // TODO: replace with IEvents
    public event Action? EventOnConnect;
    public event Action? EventOnDisconnect;
    
    private readonly NetClient _client;
    private readonly IContainer _container;
    private readonly ILoggingService _logger;
    private readonly EventService _eventService;
    
    private NetConnection? _connection;
    private float _connectionAttemptTime;
    
    public ClientNetworkService(IContainer container, ILoggingService logger, EventService eventService) : base(logger, eventService)
    {
        _container = container;
        _logger = logger;
        _eventService = eventService;

        var config = new NetPeerConfiguration("Ages of Automation")
        {
            PingInterval = 1f,
            ConnectionTimeout = 5f
        };
        
        _client = new NetClient(config);
        _client.Start();
    }

    protected override void OnOutgoing(OutgoingEvent e)
    {
        // Check if this is a server only event
        if (e.NetworkEvent.Receivers != null) return;
        SendMessage(_connection!, e.NetworkEvent);
    }
    
    public void Connect(string ip, ushort port)
    {
        // Create a random ID and send it in the approval request message
        var msg = _client.CreateMessage();

        var playerService = _container.Resolve<IPlayerService>();
        
        var playerName = $"Player {playerService.PlayerId}";

        msg.Write(playerService.PlayerId);
        msg.Write(playerName);

        _connection = _client.Connect(ip, port, msg);
    }

    public void Disconnect()
    {
        _client.Disconnect("Client has left the server.");
        _client.FlushSendQueue();
        Thread.Sleep(100);
    }

    public override void Update(float deltaTime)
    {
        while (_client.ReadMessage() is { } incomingMsg)
        {
            switch (incomingMsg.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    var status = (NetConnectionStatus) incomingMsg.ReadByte();
                    var reason = incomingMsg.ReadString();

                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (status)
                    {
                        case NetConnectionStatus.Connected:
                            EventOnConnect?.Invoke();
                            continue;
                        case NetConnectionStatus.Disconnected:
                            EventOnDisconnect?.Invoke();
                            _connection = null;
                            continue;
                    }

                    OnStatusChange(status, reason);

                    break;
                case NetIncomingMessageType.Data:
                    var e = OnData(incomingMsg);
                    if (e != null) _eventService.Incoming(e);
                    break; 
            }
        }

        if (_client.ConnectionStatus != NetConnectionStatus.Disconnected) return;

        if (_connectionAttemptTime < 5f)
        {
            _connectionAttemptTime += deltaTime;
            return;
        }

        Disconnect();
    }
    
    private void OnStatusChange(NetConnectionStatus newStatus, string reason)
    {
        _logger.Information($"Client status changed to {newStatus} for the reason: {reason}");
    }
    
    public override void Dispose()
    {
        base.Dispose();
        _client.Shutdown(new NetReason(string.Empty));
        GC.SuppressFinalize(this);
    }
}