using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace LitterboxEngine.Graphics.Vulkan;

public class Surface: IDisposable
{
    private readonly KhrSurface _khrSurface;
    private readonly SurfaceKHR _vkSurface;
    private readonly Instance _instance;
    
    public unsafe Surface(Vk vk, Instance instance, Window window)
    {
        _instance = instance;
        
        var vkNonDispatchableHandle = stackalloc VkNonDispatchableHandle[1];
        
        var isSurfaceCreated = window.Glfw.CreateWindowSurface(_instance.VkInstance.ToHandle(), window.WindowHandle, null, vkNonDispatchableHandle);
        if (isSurfaceCreated != (int)Result.Success)
            throw new Exception($"Failed to create window surface with error code: {isSurfaceCreated}");

        _vkSurface = vkNonDispatchableHandle[0].ToSurface();
        
        if (!vk.TryGetInstanceExtension(_instance.VkInstance, out _khrSurface))
            throw new Exception("Failed to instantiate KHR surface extension");
    }

    public unsafe void Dispose()
    {
        _khrSurface.DestroySurface(_instance.VkInstance, _vkSurface, null);
        GC.SuppressFinalize(this);
    }
}