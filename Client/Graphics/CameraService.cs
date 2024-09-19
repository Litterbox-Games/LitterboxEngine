using System.Numerics;
using Client.Graphics.Input;
using Common.DI;
using Common.DI.Attributes;

namespace Client.Graphics;

public class CameraService : ITickableService
{
    public readonly Camera Camera;
    public Vector2 Target;

    private float _scaleFactor;

    public CameraService(IWindowService windowService)
    {
        Camera = new Camera(Vector2.Zero, new Vector2(windowService.Width, windowService.Height));
        RecalculateCamera(windowService.Width, windowService.Height);
        windowService.OnResize += RecalculateCamera;
    }

    private const float BaseWidth = 20f; // desired width in world units
    private const float BaseHeight = 11.25f; // desired height in world units

    private void RecalculateCamera(int width, int height)
    {
        const float targetAspect = BaseWidth / BaseHeight;
        var windowAspect = (float)width / height;

        _scaleFactor = windowAspect > targetAspect ? height / BaseHeight : width / BaseWidth;
        _scaleFactor = MathF.Floor(_scaleFactor);

        Camera.Size = new Vector2(width / _scaleFactor, height / _scaleFactor);
        Camera.Update();
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        // if (Camera.Position == Target) return;
        
        // var difference = Target - Camera.Position;
        // var distance = difference.Length();
        
        // const float smoothFactor = 5f;
        // const float snapThreshold = 0.1f;
        
        // Camera.Position = distance > snapThreshold
        //     ? Vector2.Lerp(Camera.Position, Target, 1 - MathF.Exp(-smoothFactor * deltaTime))
        //     : Target;

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