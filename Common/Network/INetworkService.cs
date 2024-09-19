using Common.DI;
using Lidgren.Network;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Network;

public interface INetworkService : ITickableService
{
    ulong PlayerId { get; }
    
    void SendMessage(NetConnection connection, INetworkMessage message);
    void SendMessage(IEnumerable<NetConnection> connections, INetworkMessage message);
    void RegisterMessageType<T>() where T : INetworkMessage, new();
    void RegisterMessageHandle<T>(OnMessage handle) where T : INetworkMessage, new();
}