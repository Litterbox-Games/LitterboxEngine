using System.Numerics;
using Client.Graphics.Input;
using Common.DI;
using Common.Mathematics;

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
        Camera.Position = Target + _windowService.Size.ToVector2() / _scaleFactor / 2;
        Camera.Position *= _scaleFactor;
        Camera.Position = Camera.Position.Round();
        Camera.Position /= _scaleFactor;

        Camera.Update();
    }

    /// <inheritdoc />
    public void Draw()
    {
        
    }

    public Vector2 ScreenToWorldPosition(Vector2 position)
    {
        var screenSpace = position / _windowService.Size.ToVector2() * 2 - Vector2.One;
        var clipSpace = new Vector4(screenSpace, Camera.NearPlane, 1);
        Matrix4x4.Invert(Camera.ViewMatrix, out var inverseViewMatrix);
        var worldSpace = Vector4.Transform(clipSpace, inverseViewMatrix);
        return new Vector2(worldSpace.X, worldSpace.Y);
    }
}