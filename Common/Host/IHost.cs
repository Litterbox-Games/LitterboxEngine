using System.Runtime.CompilerServices;
using Common.Core;
using Common.Core.Attributes;

[assembly: InternalsVisibleTo("Client")]

namespace Common.Host;

public interface IHost: IDisposable
{
    public IContainer EngineContainer { get; }
    public IContainer? GameContainer { get; set; }
    
    List<(EPriority, IUpdatable)> EngineUpdatables { get; }
    List<(EPriority, IUpdatable)> GameUpdatables { get; }

    public void Start(EGameMode gameMode);
    
    public void Stop();
    
    public void Update(float deltaTime);
}