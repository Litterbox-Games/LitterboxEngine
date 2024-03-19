using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class Semaphore:  IDisposable
{
    public Silk.NET.Vulkan.Semaphore VkSemaphore;
    
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;
    
    public unsafe Semaphore(Vk vk, LogicalDevice logicalDevice)
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