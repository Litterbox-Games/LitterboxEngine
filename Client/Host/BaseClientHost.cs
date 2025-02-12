using Client.Graphics;
using Common.Host;

namespace Client.Host;

public class BaseClientHost: BaseHost
{
    private readonly List<IDrawable> _drawables = [];

    protected BaseClientHost(EGameMode gameMode): base(gameMode)
    {
        Container.FilterRegistries<IDrawable>((drawable, _) =>
        {
            _drawables.Add(drawable);
        });
    }
    
    public void Draw(Renderer renderer)
    {
        _drawables.ForEach(drawable => drawable.Draw(renderer));
    }
}