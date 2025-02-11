using System.Numerics;
using Client.Graphics.Input;
using Common.DI;
using Common.Mathematics;

namespace Client.Graphics;

public class CameraService : IService, IUpdatable
{
    private Window? _window;
    
    public readonly Camera Camera;
    public Vector2 Target;

    private int _scaleFactor;

    public CameraService()
    {
        Camera = new Camera(Vector2.Zero, new Vector2(1920, 1080));
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
        if (_window == null) return;
        
        Camera.Position = Target + _window.Size.ToVector2() / _scaleFactor / 2;
        Camera.Position *= _scaleFactor;
        Camera.Position = Camera.Position.Round();
        Camera.Position /= _scaleFactor;

        Camera.Update();
    }

    public void SetWindow(Window window)
    {
        if (_window != null)
        {
            window.OnResize -= RecalculateCamera;
        }
        
        _window = window;
        RecalculateCamera(window.Width, window.Height);
        window.OnResize += RecalculateCamera;   
    }
    
    public Vector2 ScreenToWorldPosition(Vector2 position)
    {
        if (_window == null) return Vector2.Zero;
        
        var screenSpace = position / _window.Size.ToVector2() * 2 - Vector2.One;
        var clipSpace = new Vector4(screenSpace, Camera.NearPlane, 1);
        Matrix4x4.Invert(Camera.ViewMatrix, out var inverseViewMatrix);
        var worldSpace = Vector4.Transform(clipSpace, inverseViewMatrix);
        return new Vector2(worldSpace.X, worldSpace.Y);
    }
}