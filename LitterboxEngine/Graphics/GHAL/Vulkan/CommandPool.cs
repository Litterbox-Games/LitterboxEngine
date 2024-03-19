using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class CommandPool: IDisposable
{
    public readonly Silk.NET.Vulkan.CommandPool VkCommandPool;

    private readonly Vk _vk;
    public readonly LogicalDevice LogicalDevice;


    public  unsafe CommandPool(Vk vk, LogicalDevice logicalDevice, uint queueFamilyIndex)
    {
        _vk = vk;
        LogicalDevice = logicalDevice;
        
        CommandPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
            QueueFamilyIndex = queueFamilyIndex,
        };

        var result = _vk.CreateCommandPool(LogicalDevice.VkLogicalDevice, poolInfo, null, out VkCommandPool); 
        if (result != Result.Success)
            throw new Exception($"Failed to create command pool with error: {result.ToString()}");
    }

    public unsafe void Dispose()
    {
        _vk.DestroyCommandPool(LogicalDevice.VkLogicalDevice, VkCommandPool, null);
        GC.SuppressFinalize(this);
    }
}