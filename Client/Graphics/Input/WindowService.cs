using Common.DI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Client.Graphics.Input;

public class WindowService : IService, IDisposable
{
    public string Title { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public event Action<int, int>? OnResize;

    public event Action<float>? OnUpdate;
    public event Action<float>? OnDraw;

    public readonly IWindow Window;
    public readonly IInputContext Input;
    
    public WindowService()
    {
        Title = "Litterbox Engine";
        Width = 1920;
        Height = 1080;
        
        var options = WindowOptions.DefaultVulkan;
        options.Title = Title;
        options.Size = new Vector2D<int>(Width, Height);
        options.IsEventDriven = false;
        options.FramesPerSecond = 144;
        options.UpdatesPerSecond = 60;
        
        Window = Silk.NET.Windowing.Window.Create(options); 
        Window.Initialize();
        
        Window.FramebufferResize += Resize;
        Window.Update += deltaTime => OnUpdate?.Invoke((float)deltaTime);
        Window.Render += deltaTime => OnDraw?.Invoke((float)deltaTime);

        Input = Window.CreateInput();
    }

    private void Resize(Vector2D<int> size)
    {
        Width = size.X;
        Height = size.Y;
        OnResize?.Invoke(Width, Height);
    }

    public void SetShouldClose()
    {
        Window.Close();
    }

    public void Run()
    {
        Window.Run();
    }

    public void Dispose()
    {
        Input.Dispose();
        Window.Dispose();
        GC.SuppressFinalize(this);
    }
}