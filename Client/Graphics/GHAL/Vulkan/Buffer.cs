using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class Buffer: GHAL.Buffer
{
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;
    private readonly CommandPool _commandPool;
    private readonly Queue _queue;
    private readonly StagingBuffer _stagingBuffer;

    public readonly ulong Size;
    
    public readonly Silk.NET.Vulkan.Buffer VkBuffer;
    public readonly DeviceMemory VkBufferMemory;

    public unsafe Buffer(Vk vk, LogicalDevice logicalDevice, BufferDescription description, MemoryPropertyFlags properties, CommandPool commandPool, Queue queue)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        _commandPool = commandPool;
        _queue = queue;
        Size = description.Size;
        
        BufferCreateInfo bufferInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = Size,
            Usage = BufferUsageFlagsFromBufferUsage(description.Usage),
            SharingMode = SharingMode.Exclusive
        };
        
        var result = _vk.CreateBuffer(_logicalDevice.VkLogicalDevice, bufferInfo, null, out VkBuffer);
        if (result != Result.Success)
            throw new Exception($"Failed to create vertex buffer with error: {result.ToString()}");

        _vk.GetBufferMemoryRequirements(_logicalDevice.VkLogicalDevice, VkBuffer, out var memRequirements);

        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = MemoryTypeFromProperties(memRequirements.MemoryTypeBits, properties)
        };
        
        result = _vk.AllocateMemory(_logicalDevice.VkLogicalDevice, allocateInfo, null, out VkBufferMemory); 
        if (result != Result.Success)
            throw new Exception($"Failed to allocate vertex buffer memory with error: {result.ToString()}");

        result = _vk.BindBufferMemory(_logicalDevice.VkLogicalDevice, VkBuffer, VkBufferMemory, 0);
        if (result != Result.Success)
            throw new Exception($"Failed to bind buffer memory with error: {result.ToString()}");

        _stagingBuffer = new StagingBuffer(_vk, _logicalDevice, _commandPool, _queue, Size);
    }

    public override unsafe void Update<T>(ulong offset, T[] data)
    {
        var dataSize = (ulong)(sizeof(T) * data.Length);
        
        void* dataPtr;
        _vk.MapMemory(_logicalDevice.VkLogicalDevice, _stagingBuffer.VkBufferMemory, 0, dataSize, 0, &dataPtr);
        data.AsSpan().CopyTo(new Span<T>(dataPtr, data.Length));
        _vk.UnmapMemory(_logicalDevice.VkLogicalDevice, _stagingBuffer.VkBufferMemory);
        
        using var commandBuffer = new CommandBuffer(_vk, _commandPool, true, true);
        commandBuffer.BeginRecording();
        BufferCopy copyRegion = new() { DstOffset = offset, Size = dataSize };
        _vk.CmdCopyBuffer(commandBuffer.VkCommandBuffer, _stagingBuffer.VkBuffer, VkBuffer, 1, copyRegion);
        commandBuffer.EndRecording();
        
        using var fence = new Fence(_vk, _logicalDevice, true);
        fence.Reset();
        _queue.Submit(commandBuffer, null, fence);
        fence.Wait();
    }
    
    public override unsafe void Update<T>(ulong offset, T data)
    {
        var dataSize = (ulong)sizeof(T);
        
        void* dataPtr;
        _vk.MapMemory(_logicalDevice.VkLogicalDevice, _stagingBuffer.VkBufferMemory, 0, dataSize, 0, &dataPtr);
        new Span<T>(dataPtr, 1).Fill(data);
        _vk.UnmapMemory(_logicalDevice.VkLogicalDevice, _stagingBuffer.VkBufferMemory);


        using var commandBuffer = new CommandBuffer(_vk, _commandPool, true, true);
        commandBuffer.BeginRecording();
        BufferCopy copyRegion = new() { DstOffset = offset, Size = dataSize };
        _vk.CmdCopyBuffer(commandBuffer.VkCommandBuffer, _stagingBuffer.VkBuffer, VkBuffer, 1, copyRegion);
        commandBuffer.EndRecording();

        using var fence = new Fence(_vk, _logicalDevice, true);
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

    private static BufferUsageFlags BufferUsageFlagsFromBufferUsage(BufferUsage usage)
    {
        return usage switch
        {
            // These usages are reserved for non-staging buffers, TransferDstBit is required to allow staging buffers to copy to buffers using them
            BufferUsage.Vertex => BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferDstBit,
            BufferUsage.Index => BufferUsageFlags.IndexBufferBit | BufferUsageFlags.TransferDstBit,
            BufferUsage.Uniform => BufferUsageFlags.UniformBufferBit | BufferUsageFlags.TransferDstBit,
            _ => throw new ArgumentOutOfRangeException(nameof(usage), usage, null)
        };
    }

    

    public override unsafe void Dispose()
    {
        _stagingBuffer.Dispose();
        _vk.DestroyBuffer(_logicalDevice.VkLogicalDevice, VkBuffer, null);
        _vk.FreeMemory(_logicalDevice.VkLogicalDevice, VkBufferMemory, null);
        GC.SuppressFinalize(this);
    }
}