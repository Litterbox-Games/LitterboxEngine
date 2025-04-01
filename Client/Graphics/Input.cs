using System.Numerics;
using Silk.NET.Input;

namespace Client.Graphics;

public class Input(Window window)
{
    private readonly IInputContext _input = window.Input;

    // TODO: give the user the option to choose a mouse
    public Vector2 MousePosition => _input.Mice[0].Position;
    public bool IsMouseDown(MouseButton button) => _input.Mice[0].IsButtonPressed(button);
    
    public bool IsKeyDown(Key key) => _input.Keyboards[0].IsKeyPressed(key);
    
}