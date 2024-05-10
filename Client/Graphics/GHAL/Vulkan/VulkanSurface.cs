using Client.Graphics.Input;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanSurface: IDisposable
{
    public readonly KhrSurface KhrSurface;
    public readonly SurfaceKHR VkSurface;
    public readonly SurfaceFormatKHR Format;
    private readonly VulkanInstance _instance;
    
    public unsafe VulkanSurface(Vk vk, VulkanInstance instance, VulkanPhysicalDevice physicalDevice, GlfwWindowService windowService)
    {
        _instance = instance;
        
        var vkNonDispatchableHandle = stackalloc VkNonDispatchableHandle[1];
        
        var isSurfaceCreated = windowService.Glfw.CreateWindowSurface(_instance.VkInstance.ToHandle(), windowService.WindowHandle, null, vkNonDispatchableHandle);
        if (isSurfaceCreated != (int)Result.Success)
            throw new Exception($"Failed to create window surface with error code: {isSurfaceCreated}");

        VkSurface = vkNonDispatchableHandle[0].ToSurface();
        
        if (!vk.TryGetInstanceExtension(_instance.VkInstance, out KhrSurface))
            throw new Exception("Failed to instantiate KHR surface extension");
        
        // Calculate Surface Format
        uint formatCount = 0;
        var result = KhrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice.VkPhysicalDevice, 
            VkSurface, ref formatCount, null);
        
        if (result != Result.Success)
            throw new Exception($"Failed to get physical device formats with error: {result.ToString()}.");
        
        if (formatCount == 0)
            throw new Exception("No available formats for selected physical device");
        
        var formats = stackalloc SurfaceFormatKHR[(int)formatCount];
        result = KhrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice.VkPhysicalDevice, 
            VkSurface, ref formatCount, formats);
        
        if (result != Result.Success)
            throw new Exception($"Failed to get physical device surface formats with error: {result.ToString()}.");

        Format = formats[0];
        for (var i = 0; i < formatCount; i++)
        {
            var format = formats[i];
            if (format is {Format: Silk.NET.Vulkan.Format.B8G8R8A8Srgb, ColorSpace: ColorSpaceKHR.SpaceSrgbNonlinearKhr}) 
                Format = format;
        }
    }

    public unsafe void Dispose()
    {
        KhrSurface.DestroySurface(_instance.VkInstance, VkSurface, null);
        KhrSurface.Dispose();
        GC.SuppressFinalize(this);
    }
}