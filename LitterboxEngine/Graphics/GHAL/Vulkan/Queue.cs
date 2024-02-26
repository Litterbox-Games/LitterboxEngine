using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.Vulkan;

public class Queue
{
    private readonly Vk _vk;
    public readonly Silk.NET.Vulkan.Queue VkQueue;
    public readonly uint QueueFamilyIndex;     //TODO: change this to uint because better
    
    protected Queue(Vk vk, LogicalDevice logicalDevice, uint queueFamilyIndex, uint queueIndex)
    {
        _vk = vk;

        QueueFamilyIndex = queueFamilyIndex;
        vk.GetDeviceQueue(logicalDevice.VkLogicalDevice, QueueFamilyIndex, queueIndex, out VkQueue);
    }

    public unsafe void Submit(CommandBuffer commandBuffer, SwapChainSyncSemaphores syncSemaphores, Fence fence)
    {
        var vkCommandBuffer = commandBuffer.VkCommandBuffer;
        var signalSemaphores = stackalloc[] { syncSemaphores.RenderComplete.VkSemaphore };
        var waitSemaphores = stackalloc[] {syncSemaphores.ImageAcquisition.VkSemaphore};
        var stageMask = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &vkCommandBuffer,
            SignalSemaphoreCount = 1,
            PSignalSemaphores = signalSemaphores,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = waitSemaphores,
            PWaitDstStageMask = stageMask 
        };
        
        var result = _vk.QueueSubmit(VkQueue, 1, submitInfo, fence.VkFence);
        if (result != Result.Success)
            throw new Exception($"Failed to submit command to queue with error: {result.ToString()}");

    }
    
    public void WaitIdle()
    {
        var result = _vk.QueueWaitIdle(VkQueue);
        if (result != Result.Success)
            throw new Exception($"Failed to wait for queue to idle with error {result.ToString()}");
    }
}

public class GraphicsQueue: Queue
{
    public GraphicsQueue(Vk vk, LogicalDevice logicalDevice, uint queueIndex): 
        base(vk, logicalDevice, logicalDevice.GraphicsQueueFamilyIndex, queueIndex) {}
}

public class PresentQueue : Queue
{
    public PresentQueue(Vk vk, LogicalDevice logicalDevice, Surface surface, uint queueIndex):
        base(vk, logicalDevice, logicalDevice.PhysicalDevice.GetPresentQueueFamilyIndex(surface), queueIndex) {}
}