using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Client.Graphics.GHAL.Vulkan;

public class PhysicalDevice
{
    private readonly Vk _vk;
    public readonly Instance Instance;
    public readonly Silk.NET.Vulkan.PhysicalDevice VkPhysicalDevice;
    private readonly PhysicalDeviceProperties _vkPhysicalDeviceProperties;
    public readonly string[] AvailableDeviceExtensions;
    public readonly QueueFamilyProperties[] VkQueueFamilyProperties;
    private readonly PhysicalDeviceFeatures _vkPhysicalDeviceFeatures;
    private readonly PhysicalDeviceMemoryProperties _vkMemoryProperties;

    public string Name
    {
       get { unsafe { fixed (byte* namePtr = _vkPhysicalDeviceProperties.DeviceName) 
            return SilkMarshal.PtrToString((nint)namePtr)!; }}
    }

    private unsafe PhysicalDevice(Vk vk, Instance instance, Silk.NET.Vulkan.PhysicalDevice vkPhysicalDevice)
    {
        _vk = vk;
        Instance = instance;
        VkPhysicalDevice = vkPhysicalDevice;
        
        // Device properties
        _vk.GetPhysicalDeviceProperties(VkPhysicalDevice, out _vkPhysicalDeviceProperties);
        
        // Device extensions
        uint extensionCount = 0;
        var result = _vk.EnumerateDeviceExtensionProperties(VkPhysicalDevice, (string)null!, ref extensionCount, null);
        if (result != Result.Success)
            throw new Exception($"Failed to get the number of device extension properties with error: {result.ToString()}");

        Span<ExtensionProperties> deviceExtensions = new ExtensionProperties[extensionCount];
        result = _vk.EnumerateDeviceExtensionProperties(VkPhysicalDevice, (string)null!, &extensionCount, deviceExtensions);
        if (result != Result.Success)
            throw new Exception($"Failed to get extension properties with error: {result.ToString()}");

        AvailableDeviceExtensions = deviceExtensions.ToArray()
            .Select(ext =>
            {
                // ReSharper disable once ConvertToLambdaExpression
                return SilkMarshal.PtrToString((nint) ext.ExtensionName)!;
            }).ToArray();

        // Queue family properties
        uint queueCount = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(VkPhysicalDevice, ref queueCount, null);
        Span<QueueFamilyProperties> queueFamilyProperties = VkQueueFamilyProperties = new QueueFamilyProperties[queueCount]; 
        _vk.GetPhysicalDeviceQueueFamilyProperties(VkPhysicalDevice, &queueCount, queueFamilyProperties);
        
        // Device features
        _vk.GetPhysicalDeviceFeatures(VkPhysicalDevice, out _vkPhysicalDeviceFeatures);

        // Memory information and properties
        _vk.GetPhysicalDeviceMemoryProperties(VkPhysicalDevice, out _vkMemoryProperties);
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

        var supportedDevices = new List<PhysicalDevice>();
        PhysicalDevice? selectedPhysicalDevice = null;
        foreach (var vkPhysicalDevice in vkPhysicalDevices)
        {
            var physicalDevice = new PhysicalDevice(vk, instance, vkPhysicalDevice);

            var deviceName = physicalDevice.Name;

            if (!physicalDevice.HasGraphicsQueueFamily() || !physicalDevice.HasKhrSwapChainExtension()) continue;

            if (preferredDeviceName != null && preferredDeviceName == deviceName)
            {
                selectedPhysicalDevice = physicalDevice;
                break;
            }
            supportedDevices.Add(physicalDevice);
        }

        // If we didnt find the preferred device, just fall back to the first supported device if possible
        // We also prioritize discrete gpus if available
        selectedPhysicalDevice ??= supportedDevices
            .Where(device => device._vkPhysicalDeviceProperties.DeviceType == PhysicalDeviceType.DiscreteGpu)
            .FirstOrDefault(supportedDevices.FirstOrDefault((PhysicalDevice)null!));

        if (selectedPhysicalDevice == null)
            throw new Exception("Failed to find a suitable physical device");

        return selectedPhysicalDevice;
    }
    
    public uint GetGraphicsQueueFamilyIndex()
    {
        // Return index of first queue family with graphics bit
        var index = VkQueueFamilyProperties
            .Select((familyProps, i) => (familyProps.QueueFlags.HasFlag(QueueFlags.GraphicsBit), i))
            .Where(tuple => tuple.Item1)
            .Select(tuple => tuple.i)
            .FirstOrDefault(-1);
        
        if (index < 0)
            throw new Exception("Failed to get graphics queue family index");

        return (uint)index;
    }

    public uint GetPresentQueueFamilyIndex(Surface surface)
    {
        var index = VkQueueFamilyProperties
            .Select((_, i) =>
            {
                var result = surface.KhrSurface.GetPhysicalDeviceSurfaceSupport(VkPhysicalDevice, (uint) i,
                    surface.VkSurface, out var isSupported);
                if (result != Result.Success)
                    throw new Exception($"Failed to get physical device surface support: {result.ToString()}"); 
                
                return (isSupported, i);
            })
            .Where(tuple => tuple.Item1)
            .Select(tuple => tuple.i)
            .FirstOrDefault(-1);

        if (index < 0)
            throw new Exception("Failed to get presentation queue family index");

        return (uint)index;
    }

    private bool HasGraphicsQueueFamily()
    {
        return VkQueueFamilyProperties.Any(familyProps => familyProps.QueueFlags.HasFlag(QueueFlags.GraphicsBit));
    }

    private bool HasKhrSwapChainExtension()
    {
        return _vk.IsDeviceExtensionPresent(VkPhysicalDevice, KhrSwapchain.ExtensionName);
    }
}