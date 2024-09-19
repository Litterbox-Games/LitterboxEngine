using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanStagingBuffer: IDisposable
{
    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;
    private readonly VulkanCommandPool _commandPool;
    private readonly VulkanQueue _queue;
    
    public readonly ulong Size;
    
    public readonly Silk.NET.Vulkan.Buffer VkBuffer;
    public readonly DeviceMemory VkBufferMemory;

    public unsafe VulkanStagingBuffer(Vk vk, VulkanLogicalDevice logicalDevice, VulkanCommandPool commandPool, VulkanQueue queue, ulong size)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        _commandPool = commandPool;
        _queue = queue;
        Size = size;
        
        BufferCreateInfo stagingBufferInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = Size,
            Usage = BufferUsageFlags.TransferSrcBit,
            SharingMode = SharingMode.Exclusive
        };
        
        var result = _vk.CreateBuffer(_logicalDevice.VkLogicalDevice, stagingBufferInfo, null, out VkBuffer);
        if (result != Result.Success)
            throw new Exception($"Failed to create vertex buffer with error: {result.ToString()}");

        _vk.GetBufferMemoryRequirements(_logicalDevice.VkLogicalDevice, VkBuffer, out var stagingMemRequirements);

        MemoryAllocateInfo stagingAllocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = stagingMemRequirements.Size,
            MemoryTypeIndex = MemoryTypeFromProperties(stagingMemRequirements.MemoryTypeBits, 
                MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
        };
        
        result = _vk.AllocateMemory(_logicalDevice.VkLogicalDevice, stagingAllocateInfo, null, out VkBufferMemory); 
        if (result != Result.Success)
            throw new Exception($"Failed to allocate vertex buffer memory with error: {result.ToString()}");

        result = _vk.BindBufferMemory(_logicalDevice.VkLogicalDevice, VkBuffer, VkBufferMemory, 0);
        if (result != Result.Success)
            throw new Exception($"Failed to bind buffer memory with error: {result.ToString()}");
    }

    public void CopyTo(VulkanImage image)
    {
        using var commandBuffer = new VulkanCommandBuffer(_vk, _commandPool, true, true);
        commandBuffer.BeginRecording();
        
        BufferImageCopy region = new()
        {
            BufferOffset = 0,
            BufferRowLength = 0,
            BufferImageHeight = 0,
            ImageSubresource =
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1,
            },
            ImageOffset = new Offset3D(0, 0, 0),
            ImageExtent = new Extent3D(image.Width, image.Height, 1),

        };

        _vk.CmdCopyBufferToImage(commandBuffer.VkCommandBuffer, VkBuffer, image.VkImage, ImageLayout.TransferDstOptimal, 1, region);
        commandBuffer.EndRecording();
        
        using var fence = new VulkanFence(_vk, _logicalDevice, true);
        fence.Reset();
        _queue.Submit(commandBuffer, null, fence);
        fence.Wait();
    }

    // Utility function to convert memory properties into memory type
    private uint MemoryTypeFromProperties(uint typeFilter, MemoryPropertyFlags properties)
    {
        _vk.GetPhysicalDeviceMemoryProperties(_logicalDevice.PhysicalDevice.VkPhysicalDevice, out var memProperties);

        for (var i = 0; i < memProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & 1) == 1 && (memProperties.MemoryTypes[i].PropertyFlags & properties) == properties)
                return (uint) i;

            typeFilter >>= 1;
        }

        throw new Exception("Failed to find suitable memory type");
    }

    public unsafe void Dispose()
    {
        _vk.DestroyBuffer(_logicalDevice.VkLogicalDevice, VkBuffer, null);
        _vk.FreeMemory(_logicalDevice.VkLogicalDevice, VkBufferMemory, null);
        GC.SuppressFinalize(this);
    }
}