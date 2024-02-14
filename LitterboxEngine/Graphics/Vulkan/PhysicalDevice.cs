using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace LitterboxEngine.Graphics.Vulkan;

public class PhysicalDevice
{
    private readonly Vk _vk;
    private readonly Silk.NET.Vulkan.PhysicalDevice _vkPhysicalDevice;
    private readonly PhysicalDeviceProperties _vkPhysicalDeviceProperties;
    private readonly ExtensionProperties[] _vkDeviceExtensions;
    private readonly QueueFamilyProperties[] _vkQueueFamilyProperties;
    private readonly PhysicalDeviceFeatures _vkPhysicalDeviceFeatures;
    private readonly PhysicalDeviceMemoryProperties _vkMemoryProperties;

    public string Name
    {
       get { unsafe { fixed (byte* namePtr = _vkPhysicalDeviceProperties.DeviceName) 
            return SilkMarshal.PtrToString((nint)namePtr)!; }}
    }

    private unsafe PhysicalDevice(Vk vk, Silk.NET.Vulkan.PhysicalDevice vkPhysicalDevice)
    {
        _vk = vk;
        _vkPhysicalDevice = vkPhysicalDevice;
        
        // Device properties
        _vk.GetPhysicalDeviceProperties(_vkPhysicalDevice, out _vkPhysicalDeviceProperties);
        
        // Device extensions
        uint extensionCount = 0;
        var result = _vk.EnumerateDeviceExtensionProperties(_vkPhysicalDevice, (string)null!, ref extensionCount, null);
        if (result != Result.Success)
            throw new Exception($"Failed to get the number of device extension properties with error: {result.ToString()}");

        Span<ExtensionProperties> deviceExtensions = _vkDeviceExtensions = new ExtensionProperties[extensionCount];
        result = _vk.EnumerateDeviceExtensionProperties(_vkPhysicalDevice, (string)null!, &extensionCount, deviceExtensions);
        if (result != Result.Success)
            throw new Exception($"Failed to get extension properties with error: {result.ToString()}");

        // Queue family properties
        uint queueCount = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(_vkPhysicalDevice, ref queueCount, null);
        Span<QueueFamilyProperties> queueFamilyProperties = _vkQueueFamilyProperties = new QueueFamilyProperties[queueCount]; 
        _vk.GetPhysicalDeviceQueueFamilyProperties(_vkPhysicalDevice, &queueCount, queueFamilyProperties);
        
        // Device features
        _vk.GetPhysicalDeviceFeatures(_vkPhysicalDevice, out _vkPhysicalDeviceFeatures);

        // Memory information and properties
        _vk.GetPhysicalDeviceMemoryProperties(_vkPhysicalDevice, out _vkMemoryProperties);
    }
    
    public static unsafe PhysicalDevice SelectPreferredPhysicalDevice(Vk vk, Instance instance, string? preferredDeviceName = null)
    {
        uint physicalDeviceCount = 0;
        var result = vk.EnumeratePhysicalDevices(instance.VkInstance, ref physicalDeviceCount, null);
            
        if (result != Result.Success)
            throw new Exception($"Failed to enumerate physical devices with error: {result.ToString()}.");

        if (physicalDeviceCount == 0)
            throw new Exception($"No physical device was found");
        
        Span<Silk.NET.Vulkan.PhysicalDevice> vkPhysicalDevices = new Silk.NET.Vulkan.PhysicalDevice[physicalDeviceCount];

        result = vk.EnumeratePhysicalDevices(instance.VkInstance, &physicalDeviceCount, vkPhysicalDevices);
        
        if (result != Result.Success)
            throw new Exception($"Failed to enumerate physical devices with error: {result.ToString()}.");

        var supportedDevices = new Queue<PhysicalDevice>();
        PhysicalDevice? selectedPhysicalDevice = null;
        foreach (var vkPhysicalDevice in vkPhysicalDevices)
        {
            var physicalDevice = new PhysicalDevice(vk, vkPhysicalDevice);

            var deviceName = physicalDevice.Name;

            if (physicalDevice.HasGraphicsQueueFamily() && physicalDevice.HasKhrSwapChainExtensions())
            {
                if (preferredDeviceName != null && preferredDeviceName == deviceName)
                {
                    selectedPhysicalDevice = physicalDevice;
                    break;
                }
                supportedDevices.Enqueue(physicalDevice);
            }
        }
        
        // If we didnt find the preferred device, just fall back to the first supported device if possible
        if ((selectedPhysicalDevice ??= supportedDevices.Dequeue()) == null)
            throw new Exception("Failed to find a suitable physical device");

        return selectedPhysicalDevice;
    }

    private bool HasGraphicsQueueFamily()
    {
        return _vkQueueFamilyProperties.Any(familyProps => familyProps.QueueFlags.HasFlag(QueueFlags.GraphicsBit));
    }

    private bool HasKhrSwapChainExtensions()
    {
        return _vk.IsDeviceExtensionPresent(_vkPhysicalDevice, KhrSwapchain.ExtensionName);
    }
}