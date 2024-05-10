using Common.DI;

namespace Client.Graphics;

public interface IWindowService: IService
{
    public string Title { get; }
    // TODO: add a way to change the window size
    public int Height { get; }
    public int Width { get; }

    public event Action<int, int>? OnResize;
    
    public event Action? OnPollEvents;
    public bool ShouldClose();
    public void SetShouldClose();
    
}