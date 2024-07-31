using System.Numerics;
using Client.Graphics.Input;
using Common.DI;
using Common.DI.Attributes;

namespace Client.Graphics;

[TickablePriority(EPriority.High)]
public class CameraService : ITickableService
{
    public readonly Camera Camera;
    public float Speed = 15;
    public Vector2 Target;

    public CameraService(IWindowService windowService)
    {
        Camera = new Camera(Vector2.Zero, new Vector2(windowService.Width, windowService.Height));
        RecalculateCamera(windowService.Width, windowService.Height);
        windowService.OnResize += RecalculateCamera;
    }

    private void RecalculateCamera(int width, int height)
    {
        Camera.Size = new Vector2(width, height);
        Camera.Update();
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        if (Camera.Position == Target) return;
        
        Camera.Position = Vector2.Lerp(Camera.Position, Target, Speed * deltaTime);
        Camera.Update();
    }

    /// <inheritdoc />
    public void Draw()
    {
        
    }
}