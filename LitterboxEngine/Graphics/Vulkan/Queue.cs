using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.Vulkan;

public class Queue
{
    private readonly Vk _vk;
    public readonly Silk.NET.Vulkan.Queue VkQueue;
    
    protected Queue(Vk vk, LogicalDevice logicalDevice, int queueFamilyIndex, int queueIndex)
    {
        _vk = vk;
        vk.GetDeviceQueue(logicalDevice.VkLogicalDevice, (uint)queueFamilyIndex, (uint)queueIndex, out VkQueue);
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
    public GraphicsQueue(Vk vk, LogicalDevice logicalDevice, int queueIndex): base(vk, logicalDevice,
        logicalDevice.GraphicsQueueFamilyIndex, queueIndex) {}
}