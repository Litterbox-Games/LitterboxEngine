using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanCommandPool: IDisposable
{
    public readonly CommandPool VkCommandPool;

    private readonly Vk _vk;
    public readonly VulkanLogicalDevice LogicalDevice;


    public  unsafe VulkanCommandPool(Vk vk, VulkanLogicalDevice logicalDevice, uint queueFamilyIndex)
    {
        _vk = vk;
        LogicalDevice = logicalDevice;
        
        CommandPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
            QueueFamilyIndex = queueFamilyIndex,
        };

        var result = _vk.CreateCommandPool(LogicalDevice.VkLogicalDevice, in poolInfo, null, out VkCommandPool); 
        if (result != Result.Success)
            throw new Exception($"Failed to create command pool with error: {result.ToString()}");
    }

    public unsafe void Dispose()
    {
        _vk.DestroyCommandPool(LogicalDevice.VkLogicalDevice, VkCommandPool, null);
        GC.SuppressFinalize(this);
    }
}