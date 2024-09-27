using Common.DI;
using Common.DI.Attributes;
using Silk.NET.GLFW;

namespace Client.Graphics.Input;

public class GlfwKeyboardService: IKeyboardService
{
    // TODO: some kind of dictionary/function that converts Glfw.Keys to our Keys
    private readonly GlfwWindowService _windowService;
    public event OnKeyEvent? OnKeyEvent;
    
    private Dictionary<Key, Keys> _toGlfwKeys = new()
    {
        [Key.W] = Keys.W,
        [Key.A] = Keys.A,
        [Key.S] = Keys.S,
        [Key.D] = Keys.D,
    };
    
    private Dictionary<Keys, Key> _toKey = new()
    {
        [Keys.W] = Key.W,
        [Keys.A] = Key.A,
        [Keys.S] = Key.S,
        [Keys.D] = Key.D,
    };
    
    private Dictionary<InputAction, KeyAction> _toKeyAction = new()
    {
        [InputAction.Press] = KeyAction.Press,
        [InputAction.Release] = KeyAction.Release,
        [InputAction.Repeat] = KeyAction.Repeat
    };
    
    public unsafe GlfwKeyboardService(GlfwWindowService windowService)
    {
        /*_windowService = windowService;
        _windowService.Glfw.SetKeyCallback(_windowService.WindowHandle, (window, key, code, action, mods) =>
        {
           // if (key == Keys.Escape && action == InputAction.Release) 
           //     windowService.SetShouldClose();
        
           OnKeyEvent?.Invoke(_toKey[key], _toKeyAction[action]);
        });*/
    }
    
    public unsafe bool IsKeyDown(Key key)
    {
        return false;
        // return _windowService.Glfw.GetKey(_windowService.WindowHandle, _toGlfwKeys[key]) == (int)InputAction.Press;
    }
}