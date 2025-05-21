using Autofac.Core;
using Client.Graphics;
using Common.Core.Extensions;

namespace Client.Core.Extensions;

public static class ContainerExtensions
{
    public static List<IInputable> RegisterInputables(this Container container)
    {
        var inputables = new List<IInputable>();
        
        container.FilterRegistrations<IInputable>((inputable, _) =>
        {
            inputables.Add(inputable);
        });

        return inputables;
    }
    
    public static List<IDrawable> RegisterDrawables(this Container container)
    {
        var drawables = new List<IDrawable>();
        
        container.FilterRegistrations<IDrawable>((drawable, _) =>
        {
            drawables.Add(drawable);
        });

        return drawables;
    }
}