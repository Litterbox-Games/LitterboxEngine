using System.Numerics;
using Silk.NET.GLFW;

namespace Client.Graphics.Input;

public class GlfwMouseService: IMouseService
{
    public Vector2 CurrentPosition { get; private set; }
    public Vector2 PreviousPosition { get; private set; }
    public Vector2 Displacement { get; private set; }
    public bool InWindow { get; private set; }
    public bool IsLeftButtonPressed { get; private set; }
    public bool IsRightButtonPressed { get; private set; }

    
    public unsafe GlfwMouseService(GlfwWindowService windowService)
    {
        /*windowService.Glfw.SetCursorPosCallback(windowService.WindowHandle, 
            (_, x, y) =>
            {
                PreviousPosition = CurrentPosition;
                CurrentPosition = new Vector2((float) x, (float) y);
                Displacement = CurrentPosition - PreviousPosition;
            });
        windowService.Glfw.SetCursorEnterCallback(windowService.WindowHandle, 
            (handle, entered) => InWindow = entered);
        windowService.Glfw.SetMouseButtonCallback(windowService.WindowHandle, (_, button, action, _) =>
        {
            IsLeftButtonPressed = button == MouseButton.Left && action == InputAction.Press;
            IsRightButtonPressed = button == MouseButton.Right && action == InputAction.Press;
        });*/
    }
}