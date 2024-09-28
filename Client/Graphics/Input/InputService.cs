using Common.DI;
using Silk.NET.Input;

namespace Client.Graphics.Input;

public class InputService: IService
{
    private readonly IInputContext _input;
    
    public InputService(WindowService windowService)
    {
        _input = windowService.Input;
    }

    public bool IsKeyDown(Key key)
    {
        // TODO: give the user the option to choose a keyboard
        return _input.Keyboards[0].IsKeyPressed(key);
    }
}