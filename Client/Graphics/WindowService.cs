using Common.Core;
using Common.Mathematics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Client.Graphics;

public class WindowService: IService, IDisposable
{
    public string Title { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Vector2i Size => new(Width, Height);
    
    public event Action<int, int>? OnResize;

    public readonly IWindow InternalWindow;
    public readonly IInputContext Input;
    
    public WindowService()
    {
        Title = "Litterbox Engine";
        Width = 1920;
        Height = 1080;
        
        var options = WindowOptions.DefaultVulkan with
        {
            Title = Title,
            Size = new Vector2D<int>(Width, Height),
            IsEventDriven = false,
            UpdatesPerSecond = 60
        };
        
        InternalWindow = Window.Create(options); 
        InternalWindow.Initialize();
        
        InternalWindow.FramebufferResize += Resize;

        Input = InternalWindow.CreateInput();
    }

    private void Resize(Vector2D<int> size)
    {
        Width = size.X;
        Height = size.Y;
        OnResize?.Invoke(Width, Height);
    }

    public bool Closing() => InternalWindow.IsClosing;
    
    public void SetShouldClose(bool closing = true)  => InternalWindow.IsClosing = closing;

    public void PollEvents() => InternalWindow.DoEvents();

    public void Dispose()
    {
        InternalWindow.Dispose();
        GC.SuppressFinalize(this);
    }
}