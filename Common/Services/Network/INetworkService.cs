using Common.Core;
using Common.Services.Entities.Messages;
using Common.Services.Logging;
using Common.Services.Players;
using Common.Services.Players.Messages;
using Common.Services.World.Messages;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Services.Network;

public delegate void OnMessage<in T>(T message, NetworkPlayer? player) where T : INetworkMessage;

public interface INetworkService : IService, IUpdatable
{
    protected Dictionary<int, Type> Messages { get; }
    
    protected Dictionary<Type, List<Delegate>> MessageHandles { get; }
    
    public NetPeer NetPeer { get; }

    void SendMessage(NetConnection connection, INetworkMessage message)
    {
        var messageId = Messages.Where(x => x.Value == message.GetType()).Select(x => x.Key).First();

        var packet = NetPeer.CreateMessage();

        packet.Write(messageId);

        message.Serialize(packet);

        NetPeer.SendMessage(packet, connection, message.NetworkChannel);
    }

    void SendMessage(IEnumerable<NetConnection> connections, INetworkMessage message)
    {
        var messageId = Messages.Where(x => x.Value == message.GetType()).Select(x => x.Key).First();

        var packet = NetPeer.CreateMessage();

        packet.Write(messageId);

        message.Serialize(packet);
        
        NetPeer.SendMessage(packet, connections.ToList(), message.NetworkChannel, 0);
    }
    
    public void RegisterMessageTypes(ILoggingService? logger = null)
    {
        RegisterMessageType<PlayerConnectMessage>(logger);
        RegisterMessageType<PlayerDisconnectMessage>(logger);
        RegisterMessageType<PlayerListSyncMessage>(logger);
        
        RegisterMessageType<EntitySpawnMessage>(logger);
        RegisterMessageType<EntityDespawnMessage>(logger);
        RegisterMessageType<EntityMoveMessage>(logger);
        
        RegisterMessageType<ChunkDataMessage>(logger);
        RegisterMessageType<ChunkRequestMessage>(logger);
        RegisterMessageType<BlockUpdateMessage>(logger);
    }
    
    private void RegisterMessageType<T>(ILoggingService? logger = null) where T : INetworkMessage, new()
    {
        var hash = GetDeterministicHashCode(typeof(T).FullName!);

        logger?.Information($"Registering message '{typeof(T).Name}'");
        if (Messages.TryGetValue(hash, out var message))
        {
            logger?.Warning("Attempted to register messages sharing the same hash.");
            logger?.Warning(message.FullName!);
            logger?.Warning(typeof(T).FullName!);

            return;
        }

        Messages[hash] = typeof(T);
    }

    public void RegisterMessageHandle<T>(OnMessage<T> handle) where T : INetworkMessage, new()
    {
        if (MessageHandles.ContainsKey(typeof(T)))
            MessageHandles[typeof(T)].Add(handle);
        else
            MessageHandles[typeof(T)] = [handle];
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
}