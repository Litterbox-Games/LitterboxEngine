using Autofac;
using Common.Core;

namespace Common.Host;

public interface IHost: IDisposable
{
    public EGameMode GameMode { get; }
    public ILifetimeScope GameScope { get; set; }
    List<(EPriority, IUpdatable)> GameUpdatables { get; }

    public void Start(ILifetimeScope engineScope);
    
    public void Stop();
    
    public void Update(float deltaTime)
    {
        GameUpdatables.ForEach(updatable => updatable.Item2.Update(deltaTime));
    }
}