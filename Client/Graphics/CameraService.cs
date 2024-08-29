using System.Numerics;
using Client.Graphics.Input;
using Common.DI;
using Common.DI.Attributes;

namespace Client.Graphics;

public class CameraService : ITickableService
{
    public readonly Camera Camera;
    public float Speed = 15;
    public Vector2 Target;

    private float _scaleFactor;

    public CameraService(IWindowService windowService)
    {
        Camera = new Camera(Vector2.Zero, new Vector2(windowService.Width, windowService.Height));
        RecalculateCamera(windowService.Width, windowService.Height);
        windowService.OnResize += RecalculateCamera;
    }

    private void RecalculateCamera(int width, int height)
    {
        // / (Size.X / 20)
        _scaleFactor = width / 20f;
        Camera.Size = new Vector2(width / _scaleFactor, height / _scaleFactor);
        Camera.Update();
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        // if (Camera.Position == Target) return;
        
        // Camera.Position = Vector2.Lerp(Camera.Position, Target, Speed * deltaTime);
        // Camera.Position.X = Target.X + 0.5f - Camera.Size.X / 2;
        // Camera.Position.Y = Target.Y + 0.5f - Camera.Size.Y / 2;
        Camera.Position = Target;
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