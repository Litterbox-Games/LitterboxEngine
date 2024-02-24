using Silk.NET.GLFW;

namespace LitterboxEngine;

// TODO: Move to Graphics folder and clean up

public unsafe class Window : IDisposable
{
    public readonly Glfw Glfw;
    public readonly WindowHandle* WindowHandle;
    public readonly string Title;
    public readonly MouseInput MouseInput;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsResized { get; private set; }

    public Window(string title, GlfwCallbacks.KeyCallback? keyCallback = null)
    {
        Title = title;
        
        Glfw = Glfw.GetApi();
        if (!Glfw.Init()) 
            throw new Exception("Failed to initialize GLFW");

        if (!Glfw.VulkanSupported()) 
            throw new Exception("Cannot find a compatible Vulkan installable client driver");
        
        // TODO: Pass this stuff as params
        Glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        Glfw.WindowHint(WindowHintBool.Maximized, false);
        
        var videoMode = Glfw.GetVideoMode(Glfw.GetPrimaryMonitor());
        WindowHandle = Glfw.CreateWindow(videoMode->Width / 2, videoMode->Height / 2, title, null, null);

        if (WindowHandle == null)
            throw new Exception("Failed to create a GLFW window");

        Glfw.SetFramebufferSizeCallback(WindowHandle, (window, w, h) => Resize(w, h));

        Glfw.SetKeyCallback(WindowHandle, (window, key, code, action, mods) =>
        {
           if (key == Keys.Escape && action == InputAction.Release) 
               SetShouldClose();

           keyCallback?.Invoke(window, key, code, action, mods);
        });

        MouseInput = new MouseInput(this);
    }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
        IsResized = true;
    }

    public bool IsKeyPressed(Keys key)
    {
        return Glfw.GetKey(WindowHandle, key) == (int)InputAction.Press;
    }
    
    public void ResetResized()
    {
        IsResized = false;
    }

    public void WaitEvents()
    {
        Glfw.WaitEvents();
    }
    
    public void PollEvents()
    {
        Glfw.PollEvents();
        MouseInput.Input();
    }
    
    public bool ShouldClose()
    {
        return Glfw.WindowShouldClose(WindowHandle);
    }

    public void SetShouldClose()
    {
        Glfw.SetWindowShouldClose(WindowHandle, true);
    }

    public void Dispose()
    {
        Glfw.DestroyWindow(WindowHandle);
        Glfw.Terminate();
        Glfw.Dispose();
        GC.SuppressFinalize(this);
    }
}