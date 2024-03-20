using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class Buffer: GHAL.Buffer
{
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;

    public readonly uint Size;
    
    public readonly Silk.NET.Vulkan.Buffer VkBuffer;
    private readonly DeviceMemory _vkBufferMemory;

    public unsafe Buffer(Vk vk, LogicalDevice logicalDevice, BufferDescription description, MemoryPropertyFlags properties)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
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
        
        result = _vk.AllocateMemory(_logicalDevice.VkLogicalDevice, allocateInfo, null, out _vkBufferMemory); 
        if (result != Result.Success)
            throw new Exception($"Failed to allocate vertex buffer memory with error: {result.ToString()}");

        result = _vk.BindBufferMemory(_logicalDevice.VkLogicalDevice, VkBuffer, _vkBufferMemory, 0);
        if (result != Result.Success)
            throw new Exception($"Failed to bind buffer memory with error: {result.ToString()}");
    }

    public override unsafe void Update(uint offset, uint[] data)
    {
        void* dataPtr;
        _vk.MapMemory(_logicalDevice.VkLogicalDevice, _vkBufferMemory, offset, Size, 0, &dataPtr);
        data.AsSpan().CopyTo(new Span<uint>(dataPtr, data.Length));
        _vk.UnmapMemory(_logicalDevice.VkLogicalDevice, _vkBufferMemory);
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

    private BufferUsageFlags BufferUsageFlagsFromBufferUsage(BufferUsage usage)
    {
        return usage switch
        {
          BufferUsage.Vertex => BufferUsageFlags.VertexBufferBit,
          BufferUsage.Index => BufferUsageFlags.IndexBufferBit,
          _ => throw new ArgumentOutOfRangeException(nameof(usage), usage, null)
        };
    }

    public override unsafe void Dispose()
    {
        _vk.DestroyBuffer(_logicalDevice.VkLogicalDevice, VkBuffer, null);
        _vk.FreeMemory(_logicalDevice.VkLogicalDevice, _vkBufferMemory, null);
        GC.SuppressFinalize(this);
    }
}