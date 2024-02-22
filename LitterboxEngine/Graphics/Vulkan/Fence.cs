using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.Vulkan;

public class Fence: IDisposable
{
    public Silk.NET.Vulkan.Fence VkFence; // TODO: should never really have to interact with this directly
    
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;
    
    
    public unsafe Fence(Vk vk, LogicalDevice logicalDevice, bool isSignaled)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        FenceCreateInfo fenceInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = isSignaled ? FenceCreateFlags.SignaledBit: 0
        };

        var result = _vk.CreateFence(_logicalDevice.VkLogicalDevice, fenceInfo, null, out VkFence);
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