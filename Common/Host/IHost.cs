using System.Runtime.CompilerServices;
using Common.Core;
using Common.Core.Attributes;

[assembly: InternalsVisibleTo("Client")]

namespace Common.Host;

public interface IHost: IDisposable
{
    public IContainer? GameContainer { get; set; }
    
    List<(EPriority, IUpdatable)> GameUpdatables { get; }

    public void Start(IContainer engineContainer, EGameMode gameMode);
    
    public void Stop();
    
    public void Update(float deltaTime);
}