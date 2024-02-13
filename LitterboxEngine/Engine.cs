using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace LitterboxEngine;

public class Engine: IDisposable
{
    private readonly IGame _game;
    private readonly Window _window;
    private readonly Renderer _renderer;
    private bool _isRunning;

    private static unsafe uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
    {
        // Do not need to release this string like the others as Vulkan will release the memory automatically.
        Console.WriteLine(Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));

        return Vk.False;
    } 
    
    private static readonly string[] ValidationLayers = 
    {
#if DEBUG
        "VK_LAYER_KHRONOS_validation"
#endif
    };
    
    private static readonly string[] Extensions =
    {
        // This extension is required to work with the window surface created by GLFW.
        "VK_KHR_surface",
        "VK_KHR_win32_surface",
#if DEBUG        
        "VK_EXT_debug_utils"
#endif
    };
    
    public unsafe Engine(string title, IGame game)
    {
        _game = game;
        _window = new Window(title);
        _renderer = new Renderer(_window, Extensions, ValidationLayers, DebugCallback);
        _game.Init(_window);
    }

    private void Run()
    {
        while (_isRunning && !_window.ShouldClose())
        {
            _window.PollEvents();
            _game.Input(_window);
            _game.Update(_window);
            _renderer.DrawFrame();
        }
        
        _renderer.DeviceWaitIdle();
    }

    public void Start()
    {
        _isRunning = true;
        Run();
    }

    public void Stop()
    {
        _isRunning = false;
    }
    
    public void Dispose()
    {
        _game.Dispose();
        _renderer.Dispose();
        _window.Dispose();
        GC.SuppressFinalize(this);
    }
}