using Client.Graphics;
using Common.Host;

namespace Client.Host;

public interface IClientHost: IHost
{
    internal List<IDrawable> GameDrawables { get; }
    internal List<IInputable> GameInputables { get; }

    public void Input(InputService input);

    public void Draw(float deltaTime, RendererService renderer);
}