using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace LitterboxEngine;

public static class Program
{
    
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

    private static unsafe void Main()
    {
        using var window = new Window(1280, 720, "Vulkan Game");
        using var renderer = new Renderer(window, Extensions, ValidationLayers, DebugCallback);
        
        while (!window.ShouldClose())
        {
            window.PollEvents();

            renderer.DrawFrame();
        }
        
        renderer.DeviceWaitIdle();
    }
}