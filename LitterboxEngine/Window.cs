using Silk.NET.GLFW;

namespace LitterboxEngine;

public unsafe class Window : IDisposable
{
    public readonly WindowHandle* WindowHandle;
    public readonly Glfw Glfw;
    public readonly string Title;

    public Window (int width, int height, string title)
    {
        Title = title;
        
        Glfw = Glfw.GetApi();
        Glfw.Init();

        if (!Glfw.VulkanSupported())
            throw new Exception("Vulkan support is required to launch this application.");
        
        Glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        Glfw.WindowHint(WindowHintBool.Resizable, true);

        WindowHandle = Glfw.CreateWindow(width, height, title, null, null);
    }

    public (int, int) GetFrameBufferSize()
    {
        Glfw.GetFramebufferSize(WindowHandle, out var width, out var height);
        return (width, height);
    }

    public void SetFrameBufferResizeCallback(GlfwCallbacks.FramebufferSizeCallback callback)
    {
        Glfw.SetFramebufferSizeCallback(WindowHandle, callback);
    }
    
    public void WaitEvents()
    {
        Glfw.WaitEvents();
    }
    
    public void PollEvents()
    {
        Glfw.PollEvents();
    }
    
    public bool ShouldClose()
    {
        return Glfw.WindowShouldClose(WindowHandle);
    }

    public void Dispose()
    {
        Glfw.Dispose();
        GC.SuppressFinalize(this);
    }
}