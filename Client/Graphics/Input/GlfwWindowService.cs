using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Client.Graphics.Input;

public unsafe class GlfwWindowService : IWindowService, IDisposable
{
    // public readonly Glfw Glfw;
    // public WindowHandle* WindowHandle => Window.Handle;
    public string Title { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public event Action<int, int>? OnResize;

    public event Action<float>? OnFrame;

    public readonly IWindow Window;
    
    public GlfwWindowService()
    {
        Title = "Litterbox Engine";
        Width = 1920;
        Height = 1080;
        
        var options = WindowOptions.DefaultVulkan;
        options.Title = Title;
        options.Size = new Vector2D<int>(Width, Height);
        options.IsEventDriven = false;
        options.FramesPerSecond = 144;
        
        Window = Silk.NET.Windowing.Window.Create(options); 
        Window.Initialize();
        
        Window.FramebufferResize += Resize;
        Window.Render += deltaTime => OnFrame?.Invoke((float)deltaTime);
    }

    private void Resize(Vector2D<int> size)
    {
        Width = size.X;
        Height = size.Y;
        OnResize?.Invoke(Width, Height);
    }

    public bool IsKeyPressed(Keys key)
    {
        // var input = Window.CreateInput();
        return false;
        // return input.Keyboards[0].IsKeyPressed();
    }

    public void WaitEvents()
    {
        
    }
    
    public void PollEvents()
    {
        
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
        Window.Dispose();
        GC.SuppressFinalize(this);
    }
}