using Common.Core;
using Common.Services.Logging;
using Common.Services.Network.Events;

namespace Common.Services.Events;

public delegate void OnEvent<in T>(T e) where T : IEvent;

[Engine]
public class EventService(ILoggingService logger) : IService
{
    private readonly Dictionary<Type, List<(string, Action<IEvent>)>> _handlers = new();
    public List<Type> EventTypes => _handlers.Keys.ToList(); 
    
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
        
        foreach (var (_, handler) in handlers)
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
        
        var methodName = handler.Method.DeclaringType!.Name + "." + handler.Method.Name;
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Add((methodName, wrapped));
        }
        else
        {
            _handlers[eventType] = [(methodName, wrapped)];
        }
    }

    public void Unhandle<T>(OnEvent<T> handler) where T : IEvent
    {
        var eventType = typeof(T);
        var methodName = handler.Method.DeclaringType!.Name + "." + handler.Method.Name;

        if (!_handlers.TryGetValue(eventType, out var handlers)) return;
        
        handlers.RemoveAll(x => x.Item1 == methodName);
            
        if (handlers.Count == 0)
        {
            _handlers.Remove(eventType);
        }
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}