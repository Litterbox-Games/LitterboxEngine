using Common.DI;
using Common.Logging;
using Common.Network;
using Lidgren.Network;

namespace Client.Network;

public class ClientNetworkService : AbstractNetworkService
{
    private NetClient _client;
    private NetConnection? _connection;
    
    protected override NetPeer NetPeer => _client;
    
    public event Action? EventOnConnect;
    public event Action? EventOnDisconnect;
    
    private float _connectionAttemptTime;
    
    public ClientNetworkService(IHost host, ILoggingService logger) : base(host, logger)
    {
        var config = new NetPeerConfiguration("Ages of Automation") { 
            PingInterval = 1f,
            ConnectionTimeout = 5f
        };
        
        _client = new NetClient(config);
        _client.Start();
    }
    
    public void Connect(string ip, ushort port)
    {
        // Create a random ID and send it in the approval request message
        var msg = _client.CreateMessage();

        PlayerId = (ulong) new Random(DateTime.Now.Millisecond).Next();
        var playerName = $"Player {PlayerId}";

        msg.Write(PlayerId);
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
                    OnData(incomingMsg); 
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

    public void SendToServer(INetworkMessage message)
    {
        SendMessage(_connection!, message);
    }
    
    protected void OnData(NetIncomingMessage message)
    {
        var messageId = message.ReadInt32();

        if (!Messages.ContainsKey(messageId))
        {
            Logger.Error($"The server attempted to send an invalid message with the ID ${messageId}.");
            return;
        }

        var messageType = Messages[messageId];

        var castedMessage = (INetworkMessage) Activator.CreateInstance(messageType)!;

        castedMessage.Deserialize(message);

        if (!MessageHandles.ContainsKey(messageType))
        {
            Logger.Error(
                $"The server attempted to send an message with the ID ${messageId} that has no valid handles.");
            return;
        }

        MessageHandles[messageType].ForEach(x => x.Invoke(castedMessage, null));
    }
    
    private void OnStatusChange(NetConnectionStatus newStatus, string reason)
    {
        Logger.Information($"Client status changed to {newStatus} for the reason: {reason}");
    }
}