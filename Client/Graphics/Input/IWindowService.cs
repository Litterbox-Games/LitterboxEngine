using Common.DI;

namespace Client.Graphics.Input;

public interface IWindowService: IService
{
    public string Title { get; }
    public int Height { get; }
    public int Width { get; }
    public event Action<int, int>? OnResize;
    public event Action<float>? OnFrame;
    public void SetShouldClose();
    public void Run();
    
}