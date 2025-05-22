using Autofac;
using Client.Graphics;
using Common.Core.Extensions;

namespace Client.Core.Extensions;

public static class ILifetimeScopeExtensions
{
    public static List<IInputable> RegisterInputables(this ILifetimeScope container)
    {
        var inputables = new List<IInputable>();
        
        container.FilterRegistrations<IInputable>((inputable, _) =>
        {
            inputables.Add(inputable);
        });

        return inputables;
    }
    
    public static List<IDrawable> RegisterDrawables(this ILifetimeScope container)
    {
        var drawables = new List<IDrawable>();
        
        container.FilterRegistrations<IDrawable>((drawable, _) =>
        {
            drawables.Add(drawable);
        });

        return drawables;
    }
}