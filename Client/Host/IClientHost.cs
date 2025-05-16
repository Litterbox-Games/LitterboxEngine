using Client.Graphics;
using Common.Core;
using Common.Host;

namespace Client.Host;

public interface IClientHost: IHost
{
    internal List<IDrawable> GameDrawables { get; }

    internal List<IInputable> RegisterInputables(IContainer container)
    {
        var inputables = new List<IInputable>();
        
        container.FilterRegistrations<IInputable>((inputable, _) =>
        {
            inputables.Add(inputable);
        });

        return inputables;
    }
    
    internal List<IDrawable> RegisterDrawables(IContainer container)
    {
        var drawables = new List<IDrawable>();
        
        container.FilterRegistrations<IDrawable>((drawable, _) =>
        {
            drawables.Add(drawable);
        });
        
        return drawables;
    }

    public void Input(InputService input);

    public void Draw(float deltaTime, RendererService renderer);
}