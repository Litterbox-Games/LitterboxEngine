using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.Vulkan;

public class LogicalDevice: IDisposable
{
    private readonly Vk _vk;
    // TODO: This propery might not be required unless needed elsewhere
    private readonly PhysicalDevice _physicalDevice;
    public readonly Device VkLogicalDevice;

    private static readonly string[] RequestedExtensions =
    {
        // This extension is required to work with the window surface created by GLFW.
        "VK_KHR_swapchain",
#if MACOS
        "VK_KHR_portability_subset"
#endif
    };
    
    public unsafe LogicalDevice(Vk vk, PhysicalDevice physicalDevice)
    {
        _vk = vk;
        _physicalDevice = physicalDevice;
        
        // Check for requested extensions
        var availableExtension = _physicalDevice.AvailableDeviceExtensions;
        foreach (var extension in RequestedExtensions)
        {
            if (!availableExtension.Contains(extension))
                throw new Exception($"Requested extension \"{extension}\" is not available");
        }

        // Enable all queue families
        var queueCreateInfoCount = _physicalDevice.VkQueueFamilyProperties.Length;
        using var mem = GlobalMemory.Allocate(queueCreateInfoCount * sizeof(DeviceQueueCreateInfo));
        var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());
        var queuePriority = 1.0f;
        for (var i = 0; i < queueCreateInfoCount; i++)
        {
            queueCreateInfos[i] = new DeviceQueueCreateInfo()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = (uint)i,
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };    
        }
        
        var deviceCreateInfo = new DeviceCreateInfo()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)queueCreateInfoCount,
            PQueueCreateInfos = queueCreateInfos,
            EnabledExtensionCount = (uint)RequestedExtensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(RequestedExtensions)
        };
        
        var result = _vk.CreateDevice(_physicalDevice.VkPhysicalDevice, deviceCreateInfo, null, out VkLogicalDevice);
        
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