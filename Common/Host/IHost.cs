using System.Runtime.CompilerServices;
using Common.Core;
using Autofac.Core;

[assembly: InternalsVisibleTo("Client")]

namespace Common.Host;

public interface IHost: IDisposable
{
    public Container GameContainer { get; set; }
    
    List<(EPriority, IUpdatable)> GameUpdatables { get; }

    public void Start(Container engineContainer, EGameMode gameMode);
    
    public void Stop();
    
    public void Update(float deltaTime);
}