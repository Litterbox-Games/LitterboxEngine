using Common.Core;
using Common.Services.Events;
using Common.Services.Logging;
using Lidgren.Network;

namespace Common.Services.Network;

public abstract class NetworkService(ILoggingService logger) : IService, IUpdatable
{
    public abstract NetPeer NetPeer { get; }

    private Dictionary<int, Type> _events = new();

    public abstract void Send(INetworkEvent e);
    
    protected void SendMessage(NetConnection connection, INetworkEvent e)
    {
        var messageId = GetDeterministicHashCode(e.GetType().FullName!);

        var packet = NetPeer.CreateMessage();

        packet.Write(messageId);

        e.Serialize(packet);

        NetPeer.SendMessage(packet, connection, e.NetworkChannel);
    }

    protected void SendMessage(IEnumerable<NetConnection> connections, INetworkEvent e)
    {
        var messageId = GetDeterministicHashCode(e.GetType().FullName!);

        var packet = NetPeer.CreateMessage();

        packet.Write(messageId);

        e.Serialize(packet);
        
        NetPeer.SendMessage(packet, connections.ToList(), e.NetworkChannel, 0);
    }

    // TODO: just have INetworkService be an abstract class
    public void RegisterMessageType(Type type)
    {
        var hash = GetDeterministicHashCode(type.FullName!);

        logger?.Information($"Registering message '{type.Name}'");
        if (_events.TryGetValue(hash, out var message))
        {
            logger?.Warning("Attempted to register messages sharing the same hash.");
            logger?.Warning(message.FullName!);
            logger?.Warning(type.FullName!);

            return;
        }

        _events[hash] = type;
    }
    
    protected INetworkEvent? OnData(NetIncomingMessage message)
    {
        var messageId = message.ReadInt32();

        if (!_events.TryGetValue(messageId, out var messageType))
        {
            logger.Error($"Received an invalid message with the ID {messageId}.");
            return null;
        }
        
        var castedMessage = (INetworkEvent) Activator.CreateInstance(messageType)!;
        castedMessage.Deserialize(message);
        
        return castedMessage;
    }
    
    private static int GetDeterministicHashCode(string str)
    {
        unchecked
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;
    
            for (var i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }
    
            return hash1 + (hash2 * 1566083941);
        }
    }

    public abstract void Update(float deltaTime);
}