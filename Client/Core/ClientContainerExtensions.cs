using Client.Graphics;
using Common.Core;

namespace Client.Core;

public static class ClientContainerExtensions
{
    public static List<IInputable> RegisterInputables(this IContainer container)
    {
        var inputables = new List<IInputable>();
        
        container.FilterRegistrations<IInputable>((inputable, _) =>
        {
            inputables.Add(inputable);
        });

        return inputables;
    }
    
    public static List<IDrawable> RegisterDrawables(this IContainer container)
    {
        var drawables = new List<IDrawable>();
        
        container.FilterRegistrations<IDrawable>((drawable, _) =>
        {
            drawables.Add(drawable);
        });
        
        return drawables;
    }
}