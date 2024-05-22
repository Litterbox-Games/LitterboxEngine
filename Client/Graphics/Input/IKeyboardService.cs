using Common.DI;

namespace Client.Graphics.Input;

// TODO: This should be a tickable service with highest priority!!!!!
public delegate void OnKeyEvent(Key key, KeyAction action);
public interface IKeyboardService: IService
{
    public event OnKeyEvent? OnKeyEvent;
    public bool IsKeyDown(Key key);
}