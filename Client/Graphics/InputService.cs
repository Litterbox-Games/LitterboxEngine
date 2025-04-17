using System.Numerics;
using Common.Core;
using Silk.NET.Input;

namespace Client.Graphics;

public class InputService(WindowService windowService): IService
{
    private readonly IInputContext _input = windowService.Input;

    // TODO: give the user the option to choose a mouse
    public Vector2 MousePosition => _input.Mice[0].Position;
    public bool IsMouseDown(MouseButton button) => _input.Mice[0].IsButtonPressed(button);
    
    public bool IsKeyDown(Key key) => _input.Keyboards[0].IsKeyPressed(key);
    
}