using System.Numerics;
using Common.DI;
using Silk.NET.Input;

namespace Client.Graphics.Input;

public class InputService: IService
{
    private readonly IInputContext _input;

    public Vector2 MousePosition => _input.Mice[0].Position;
    public event Action<MouseButton, Vector2>? EventOnMouseClick; 

    public InputService(WindowService windowService)
    {
        _input = windowService.Input;
        _input.Mice[0].DoubleClickTime = 100;
        _input.Mice[0].Click += OnClick;
    }

    private void OnClick(IMouse _, MouseButton button, Vector2 position)
    {
        EventOnMouseClick?.Invoke(button, position);
    }

    public bool IsKeyDown(Key key)
    {
        // TODO: give the user the option to choose a keyboard
        return _input.Keyboards[0].IsKeyPressed(key);
    }

    public bool IsMouseDown(MouseButton button)
    {
        // TODO: give the user the option to choose a mouse
        return _input.Mice[0].IsButtonPressed(button);
    }
}