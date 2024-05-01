using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class CommandBuffer: IDisposable
{
    public Silk.NET.Vulkan.CommandBuffer VkCommandBuffer;
    
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;
    private readonly CommandPool _commandPool;

    private readonly bool _isPrimary;
    private readonly bool _isSingleUse;
    
    public CommandBuffer(Vk vk, CommandPool commandPool, bool isPrimary, bool isSingleUse)
    {
        _vk = vk;
        _logicalDevice = commandPool.LogicalDevice;
        _commandPool = commandPool;
        _isPrimary = isPrimary;
        _isSingleUse = isSingleUse;
        
        
        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool.VkCommandPool,
            Level = _isPrimary ? CommandBufferLevel.Primary : CommandBufferLevel.Secondary,
            CommandBufferCount = 1
        };
        
        var result = _vk.AllocateCommandBuffers(_logicalDevice.VkLogicalDevice, allocInfo, out VkCommandBuffer);
        if (result != Result.Success)
            throw new Exception($"Failed to allocate command buffers with error: {result.ToString()}");
    }
    
    public unsafe void BeginRecording(CommandBufferInheritanceInfo? inheritanceInfo = null)
    {
        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
        };

        if (_isSingleUse) beginInfo.Flags = CommandBufferUsageFlags.OneTimeSubmitBit;

        if (!_isPrimary)
        {
            if (inheritanceInfo == null)
                throw new Exception("Secondary command buffer must declare inheritance info");

            var pInheritanceInfo = inheritanceInfo.Value;
            beginInfo.PInheritanceInfo = &pInheritanceInfo;
            beginInfo.Flags |= CommandBufferUsageFlags.RenderPassContinueBit;
        }
        
        var result = _vk.BeginCommandBuffer(VkCommandBuffer, beginInfo);
        if (result != Result.Success)
            throw new Exception($"Failed to begin recording command buffer with error: {result.ToString()}");
    }

    public void EndRecording()
    {
        var result = _vk.EndCommandBuffer(VkCommandBuffer);
        if (result != Result.Success)
            throw new Exception($"Failed to end recording command buffer with error: {result.ToString()}");        
    }

    public void Dispose()
    {
        _vk.FreeCommandBuffers(_logicalDevice.VkLogicalDevice, _commandPool.VkCommandPool, 1, in VkCommandBuffer);
        GC.SuppressFinalize(this);
    }
}