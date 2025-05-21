using Client.Graphics;
using Common.Core.Extensions;

namespace Client.Host.Extensions;

public static class IClientHostExtensions
{
    public static void RegisterInputables(this IClientHost host)
    {
        host.GameContainer!.FilterRegistrations<IInputable>((inputable, _) =>
        {
            host.GameInputables.Add(inputable);
        });
    }
    
    public static void RegisterDrawables(this IClientHost host)
    {
        host.GameContainer!.FilterRegistrations<IDrawable>((drawable, _) =>
        {
            host.GameDrawables.Add(drawable);
        });
    }
}