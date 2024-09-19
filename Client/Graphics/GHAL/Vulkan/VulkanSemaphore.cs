using Silk.NET.Vulkan;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanSemaphore:  IDisposable
{
    public Semaphore VkSemaphore;
    
    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;
    
    public unsafe VulkanSemaphore(Vk vk, VulkanLogicalDevice logicalDevice)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        SemaphoreCreateInfo semaphoreInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo,
        };
        
        var result = _vk.CreateSemaphore(_logicalDevice.VkLogicalDevice, semaphoreInfo, null, out VkSemaphore);
        if (result != Result.Success)
            throw new Exception($"Failed to create semaphore with error: {result.ToString()}");
    }

    public unsafe void Dispose()
    {
        _vk.DestroySemaphore(_logicalDevice.VkLogicalDevice, VkSemaphore, null);
        GC.SuppressFinalize(this);
    }
}