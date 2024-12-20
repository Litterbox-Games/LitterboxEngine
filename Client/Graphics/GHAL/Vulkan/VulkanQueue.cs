﻿using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanQueue
{
    private readonly Vk _vk;
    public readonly Queue VkQueue;
    public readonly uint QueueFamilyIndex;
    
    protected VulkanQueue(Vk vk, VulkanLogicalDevice logicalDevice, uint queueFamilyIndex, uint queueIndex)
    {
        _vk = vk;

        QueueFamilyIndex = queueFamilyIndex;
        vk.GetDeviceQueue(logicalDevice.VkLogicalDevice, QueueFamilyIndex, queueIndex, out VkQueue);
    }

    public unsafe void Submit(VulkanCommandBuffer commandBuffer, SwapChainSyncSemaphores? syncSemaphores, VulkanFence fence)
    {
        var vkCommandBuffer = commandBuffer.VkCommandBuffer;
        var stageMask = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &vkCommandBuffer,
            PWaitDstStageMask = stageMask 
        };
        
        if (syncSemaphores != null)
        {
            var signalSemaphores = stackalloc[] { syncSemaphores.RenderComplete.VkSemaphore };
            var waitSemaphores = stackalloc[] { syncSemaphores.ImageAcquisition.VkSemaphore };
            
            submitInfo.SignalSemaphoreCount = 1;
            submitInfo.PSignalSemaphores = signalSemaphores;
            submitInfo.WaitSemaphoreCount = 1;
            submitInfo.PWaitSemaphores = waitSemaphores;
        }
        
        var result = _vk.QueueSubmit(VkQueue, 1, in submitInfo, fence.VkFence);
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

public class GraphicsQueue: VulkanQueue
{
    public GraphicsQueue(Vk vk, VulkanLogicalDevice logicalDevice, uint queueIndex): 
        base(vk, logicalDevice, logicalDevice.GraphicsQueueFamilyIndex, queueIndex) {}
}

public class PresentQueue : VulkanQueue
{
    public PresentQueue(Vk vk, VulkanLogicalDevice logicalDevice, VulkanSurface surface, uint queueIndex):
        base(vk, logicalDevice, logicalDevice.PhysicalDevice.GetPresentQueueFamilyIndex(surface), queueIndex) {}
}