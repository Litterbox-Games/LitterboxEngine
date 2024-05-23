using System.Numerics;
using Client.Graphics.Input;
using Common.DI;
using Common.DI.Attributes;

namespace Client.Graphics;

[TickablePriority(EPriority.High)]
public class CameraService : ITickableService
{
    public readonly Camera Camera;
    public int ScaleFactor;
    
    public CameraService(IWindowService windowService)
    {
        Camera = new Camera(Vector2.Zero, new Vector2(windowService.Width, windowService.Height));
        RecalculateCamera(windowService.Width, windowService.Height);
        windowService.OnResize += RecalculateCamera;
    }

    private void RecalculateCamera(int width, int height)
    {
        ScaleFactor = width / 20;
        Camera.Size = new Vector2(width, height) / ScaleFactor;
        Camera.Update();
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        // Camera.Update();    
    }

    /// <inheritdoc />
    public void Draw()
    {
        
    }
}