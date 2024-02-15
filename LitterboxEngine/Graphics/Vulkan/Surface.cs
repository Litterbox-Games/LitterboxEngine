using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace LitterboxEngine.Graphics.Vulkan;

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
        
        // TODO: this is how we check if a queue has presentation support. Ideally we should have a selectPreferredQueue function
        // TODO: that finds a queue with both presentation and graphics support
        // khrSurface.GetPhysicalDeviceSurfaceSupport(_physicalDevice, _queueFamilyIndex, surface, out var iSupported);
    }

    public unsafe void Dispose()
    {
        KhrSurface.DestroySurface(_instance.VkInstance, VkSurface, null);
        GC.SuppressFinalize(this);
    }
}