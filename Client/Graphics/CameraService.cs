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
        Camera.Update();    
    }

    /// <inheritdoc />
    public void Draw()
    {
        
    }
}