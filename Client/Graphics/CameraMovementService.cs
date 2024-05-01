using Common.DI;
using Common.DI.Attributes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Client.Graphics;

[TickablePriority(EPriority.High)]
public class CameraMovementService : ITickableService
{
    private Window _window = null!;

    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        const int moveSpeed = 1000;
        
        if (_window.IsKeyDown(Key.W))
        {
            _window.Camera.Position.Y -= moveSpeed * deltaTime;
        }
        
        if (_window.IsKeyDown(Key.S))
        {
            _window.Camera.Position.Y += moveSpeed * deltaTime;
        }
        
        if (_window.IsKeyDown(Key.A))
        {
            _window.Camera.Position.X -= moveSpeed * deltaTime;
        }
        
        if (_window.IsKeyDown(Key.D))
        {
            _window.Camera.Position.X += moveSpeed * deltaTime;
        }
        
        _window.Camera.Update();    
    }

    /// <inheritdoc />
    public void Draw()
    {
        
    }

    internal void SetWindow(Window window)
    {
        _window = window;
    }
}