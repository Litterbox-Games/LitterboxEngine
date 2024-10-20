using System.Numerics;
using Client.Graphics.Input;
using Common.DI;

namespace Client.Graphics;

public class CameraService : ITickableService
{
    private readonly WindowService _windowService;
    
    public readonly Camera Camera;
    public Vector2 Target;

    private int _scaleFactor;

    public CameraService(WindowService windowService)
    {
        _windowService = windowService;
        
        Camera = new Camera(Vector2.Zero, new Vector2(windowService.Width, windowService.Height));
        RecalculateCamera(windowService.Width, windowService.Height);
        windowService.OnResize += RecalculateCamera;
    }

    private void RecalculateCamera(int width, int height)
    {
        _scaleFactor = width / 20;
        Camera.Size = new Vector2(width, height) / _scaleFactor; 
        Camera.Update();
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {

        Camera.Position.X = Target.X + (float)_windowService.Width / _scaleFactor / 2;
        Camera.Position.Y = Target.Y + (float)_windowService.Height / _scaleFactor / 2;
        Camera.Position *= _scaleFactor;
        Camera.Position = new Vector2(MathF.Round(Camera.Position.X), MathF.Round(Camera.Position.Y));
        Camera.Position /= _scaleFactor;

        Camera.Update();
    }

    /// <inheritdoc />
    public void Draw()
    {
        
    }
}