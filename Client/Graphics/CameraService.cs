using Client.Graphics.Input;
using Common.DI;
using Common.DI.Attributes;

namespace Client.Graphics;

[TickablePriority(EPriority.High)]
public class CameraService : ITickableService
{
    public readonly Camera Camera;
    private readonly IKeyboardService _keyboardService;
    
    public CameraService(IKeyboardService keyboardService)
    {
        Camera = new Camera();
        _keyboardService = keyboardService;
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        const int moveSpeed = 1000;
        
        if (_keyboardService.IsKeyDown(Key.W))
        {
            Camera.Position.Y -= moveSpeed * deltaTime;
        }
        
        if (_keyboardService.IsKeyDown(Key.S))
        {
            Camera.Position.Y += moveSpeed * deltaTime;
        }
        
        if (_keyboardService.IsKeyDown(Key.A))
        {
            Camera.Position.X -= moveSpeed * deltaTime;
        }
        
        if (_keyboardService.IsKeyDown(Key.D))
        {
            Camera.Position.X += moveSpeed * deltaTime;
        }
        
        Camera.Update();    
    }

    /// <inheritdoc />
    public void Draw()
    {
        
    }
}