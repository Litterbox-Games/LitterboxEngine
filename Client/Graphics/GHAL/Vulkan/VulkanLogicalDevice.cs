using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanLogicalDevice: IDisposable
{
    private readonly Vk _vk;
    public readonly VulkanPhysicalDevice PhysicalDevice;
    public readonly Device VkLogicalDevice;
    public readonly uint GraphicsQueueFamilyIndex;

    private static readonly string[] RequestedExtensions =
    {
        // This extension is required to work with the window surface created by GLFW.
        "VK_KHR_swapchain",
#if MACOS
        "VK_KHR_portability_subset"
#endif
    };
    
    public unsafe VulkanLogicalDevice(Vk vk, VulkanPhysicalDevice physicalDevice)
    {
        _vk = vk;
        PhysicalDevice = physicalDevice;
        
        // Check for requested extensions
        var availableExtension = PhysicalDevice.AvailableDeviceExtensions;
        foreach (var extension in RequestedExtensions)
        {
            if (!availableExtension.Contains(extension))
                throw new Exception($"Requested extension \"{extension}\" is not available");
        }
        
        // Enable graphics queue family
        GraphicsQueueFamilyIndex = physicalDevice.GetGraphicsQueueFamilyIndex();
        var queuePriority = 1.0f;
        var queueCreateInfo = new DeviceQueueCreateInfo()
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = GraphicsQueueFamilyIndex,
            QueueCount = 1,
            PQueuePriorities = &queuePriority
        };            
        
        var deviceCreateInfo = new DeviceCreateInfo()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &queueCreateInfo,
            EnabledExtensionCount = (uint)RequestedExtensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(RequestedExtensions)
        };
        
        var result = _vk.CreateDevice(PhysicalDevice.VkPhysicalDevice, deviceCreateInfo, null, out VkLogicalDevice);
        
        SilkMarshal.Free((nint) deviceCreateInfo.PpEnabledExtensionNames);
            
        if (result != Result.Success)
            throw new Exception($"Failed to create Vulkan device with error: {result.ToString()}");
    }

    public void WaitIdle()
    {
        _vk.DeviceWaitIdle(VkLogicalDevice);
    }

    public unsafe void Dispose()
    {
        _vk.DestroyDevice(VkLogicalDevice, null);
        GC.SuppressFinalize(this);
    }
}