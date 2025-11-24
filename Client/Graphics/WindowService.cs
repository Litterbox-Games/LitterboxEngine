using Client.Graphics.Events;
using Common.Core;
using Common.Mathematics;
using Common.Services.Events;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Client.Graphics;

[Engine]
public class WindowService: IService, IUpdatable
{
    public string Title { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Vector2i Size => new(Width, Height);
    
    private readonly EventService _eventService;

    public readonly IWindow InternalWindow;
    public readonly IInputContext Input;
    
    public WindowService(EventService eventService)
    {
        _eventService = eventService;
        
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
        _eventService.Emit(new WindowResizeEvent(Width, Height));
    }

    public bool IsClosing() => InternalWindow.IsClosing;
    
    public void SetShouldClose(bool closing = true)  => InternalWindow.IsClosing = closing;
    
    [Priority(EPriority.Highest)]
    public void Update(float deltaTime)
    {
        InternalWindow.DoEvents();
    }

    public void Dispose()
    {
        InternalWindow.Dispose();
        GC.SuppressFinalize(this);
    }
}