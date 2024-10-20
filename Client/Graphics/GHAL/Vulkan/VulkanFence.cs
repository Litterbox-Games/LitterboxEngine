using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanFence: IDisposable
{
    public Fence VkFence;
    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;
    
    
    public unsafe VulkanFence(Vk vk, VulkanLogicalDevice logicalDevice, bool isSignaled)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        FenceCreateInfo fenceInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = isSignaled ? FenceCreateFlags.SignaledBit: 0
        };

        var result = _vk.CreateFence(_logicalDevice.VkLogicalDevice, in fenceInfo, null, out VkFence);
        if (result != Result.Success)
            throw new Exception($"Failed to create fence with error: {result.ToString()}");
    }

    public void Wait()
    {
        _vk.WaitForFences(_logicalDevice.VkLogicalDevice, 1, in VkFence, true, ulong.MaxValue);
    }

    public void Reset()
    {
        _vk.ResetFences(_logicalDevice.VkLogicalDevice, 1, in VkFence);
    }


    public unsafe void Dispose()
    {
        _vk.DestroyFence(_logicalDevice.VkLogicalDevice, VkFence, null);
        GC.SuppressFinalize(this);
    }
}