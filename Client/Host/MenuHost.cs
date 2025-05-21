using Autofac.Core;
using Client.Graphics;
using Common.Core;
using Common.Host;

namespace Client.Host;

public class MenuHost : IClientHost
{
    public Container? GameContainer { get; set; }
    
    public List<(EPriority, IUpdatable)> GameUpdatables { get; private set; } = [];
    public List<IDrawable> GameDrawables { get; private set; } = [];
    public List<IInputable> GameInputables { get; private set; } = [];
    
    public void Start(Container engineContainer, EGameMode gameMode)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public void Update(float deltaTime)
    {
        throw new NotImplementedException();
    }
    
    public void Input(InputService input)
    {
        throw new NotImplementedException();
    }

    public void Draw(float deltaTime, RendererService renderer)
    {
        throw new NotImplementedException();
    }

    public void RegisterUpdatables(Container container)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}