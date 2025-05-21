using System.Runtime.CompilerServices;
using Autofac;
using Common.Core;
using Autofac.Core;
using MoreLinq;

[assembly: InternalsVisibleTo("Client")]

namespace Common.Host;

public interface IHost: IDisposable
{
    public Container? GameContainer { get; set; }
    
    List<(EPriority, IUpdatable)> GameUpdatables { get; }

    public void Start(Container engineContainer, EGameMode gameMode);
    
    public void Stop();
    
    public void Update(float deltaTime);
}