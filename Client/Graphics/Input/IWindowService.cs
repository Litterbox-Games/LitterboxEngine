using Common.DI;

namespace Client.Graphics.Input;

public interface IWindowService: IService
{
    public string Title { get; }
    public int Height { get; }
    public int Width { get; }
    public event Action<int, int>? OnResize;
    public void PollEvents();
    public event Action? OnPollEvents;
    public bool ShouldClose();
    public void SetShouldClose();
    
}