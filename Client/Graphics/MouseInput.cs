using System.Numerics;
using Silk.NET.GLFW;

namespace Client.Graphics;

public class MouseInput
{
    public Vector2 CurrentPosition { get; private set; }
    public Vector2 PreviousPosition { get; private set; }
    public Vector2 Displacement { get; private set; }
    public bool InWindow { get; private set; }
    public bool IsLeftButtonPressed { get; private set; }
    public bool IsRightButtonPressed { get; private set; }

    
    public unsafe MouseInput(Window window)
    {
        window.Glfw.SetCursorPosCallback(window.WindowHandle, 
            (_, x, y) => CurrentPosition = new Vector2((float)x, (float)y));
        window.Glfw.SetCursorEnterCallback(window.WindowHandle, 
            (handle, entered) => InWindow = entered);
        window.Glfw.SetMouseButtonCallback(window.WindowHandle, (handle, button, action, mods) =>
        {
            IsLeftButtonPressed = button == MouseButton.Left && action == InputAction.Press;
            IsRightButtonPressed = button == MouseButton.Right && action == InputAction.Press;
        });
    }

    public void Input()
    {
        Displacement = CurrentPosition - PreviousPosition;
        PreviousPosition = CurrentPosition;
    }
}