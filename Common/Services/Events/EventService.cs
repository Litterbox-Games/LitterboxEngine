using Common.Core;
using Common.Services.Logging;
using Common.Services.Network.Events;

namespace Common.Services.Events;

public delegate void OnEvent<in T>(T e) where T : IEvent;

public class EventService(ILoggingService logger) : IService
{
    private readonly Dictionary<Type, List<Action<IEvent>>> _handlers = new();

    public void Incoming(INetworkEvent e) => CallHandlers(e);
    
    public void Outgoing(INetworkEvent e) => CallHandlers(new OutgoingEvent(e));
    
    
    public void Emit(IEvent e)
    {
        if (e is INetworkEvent networkEvent)
            Outgoing(networkEvent);
        
        CallHandlers(e);
    }

    private void CallHandlers(IEvent e)
    {
        var eventType = e.GetType();
        
        if (!_handlers.TryGetValue(eventType, out var handlers)) return;
        
        foreach (var handler in handlers)
        {
            try
            {
                handler(e);
            }
            catch (Exception error)
            {
                logger.Error($"Exception invoking handler for {eventType.Name}: {error}");
            }
        }
    }
    
    public void Handle<T>(OnEvent<T> handler) where T : IEvent
    {
        var eventType = typeof(T);
        var wrapped = new Action<IEvent>(e => handler((T)e));
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Add(wrapped);
        }
        else
        {
            if (eventType.IsAssignableTo(typeof(INetworkEvent)))
                CallHandlers(new RegisterMessageEvent(eventType));
            
            _handlers[eventType] = [wrapped];
        }
            
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}