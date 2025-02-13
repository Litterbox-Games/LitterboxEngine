using Client.Graphics;
using Common.Host;

namespace Client.Host;

public interface IClientHost: IHost
{
    internal List<IDrawable> Drawables { get; }

    internal void RegisterDrawables()
    {
        Container.FilterRegistries<IDrawable>((drawable, _) =>
        {
            Drawables.Add(drawable);
        });
    }
    
    public void Draw(Renderer renderer)
    {
        Drawables.ForEach(drawable => drawable.Draw(renderer));
    }
}