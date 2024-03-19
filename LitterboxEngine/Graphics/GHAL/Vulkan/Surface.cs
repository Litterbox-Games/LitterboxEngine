using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class Surface: IDisposable
{
    public readonly KhrSurface KhrSurface;
    public readonly SurfaceKHR VkSurface;
    private readonly Instance _instance;
    
    public unsafe Surface(Vk vk, Instance instance, Window window)
    {
        _instance = instance;
        
        var vkNonDispatchableHandle = stackalloc VkNonDispatchableHandle[1];
        
        var isSurfaceCreated = window.Glfw.CreateWindowSurface(_instance.VkInstance.ToHandle(), window.WindowHandle, null, vkNonDispatchableHandle);
        if (isSurfaceCreated != (int)Result.Success)
            throw new Exception($"Failed to create window surface with error code: {isSurfaceCreated}");

        VkSurface = vkNonDispatchableHandle[0].ToSurface();
        
        if (!vk.TryGetInstanceExtension(_instance.VkInstance, out KhrSurface))
            throw new Exception("Failed to instantiate KHR surface extension");
    }

    public unsafe void Dispose()
    {
        KhrSurface.DestroySurface(_instance.VkInstance, VkSurface, null);
        KhrSurface.Dispose();
        GC.SuppressFinalize(this);
    }
}