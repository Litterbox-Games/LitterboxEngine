using Client.Graphics;
using Common.Host;

namespace Client.Host;

public interface IClientHost: IHost
{
    internal List<IInputable> Inputables { get; }
    internal List<IDrawable> Drawables { get; }

    internal void RegisterInputables()
    {
        Container.FilterRegistries<IInputable>((inputable, _) =>
        {
            Inputables.Add(inputable);
        });
    }
    
    internal void RegisterDrawables()
    {
        Container.FilterRegistries<IDrawable>((drawable, _) =>
        {
            Drawables.Add(drawable);
        });
    }

    public void Input(InputService input)
    {
        Inputables.ForEach(inputable => inputable.Input(input));
    }
    
    public void Draw(RendererService renderer)
    {
        Drawables.ForEach(drawable => drawable.Draw(renderer));
    }
}