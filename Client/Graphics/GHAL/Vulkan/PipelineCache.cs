using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class PipelineCache: IDisposable
{
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;

    public readonly Silk.NET.Vulkan.PipelineCache VkPipelineCache;
    
    public unsafe PipelineCache(Vk vk, LogicalDevice logicalDevice)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;

        PipelineCacheCreateInfo createInfo = new()
        {
            SType = StructureType.PipelineCacheCreateInfo
        };

        var result = _vk.CreatePipelineCache(_logicalDevice.VkLogicalDevice, &createInfo, null, out VkPipelineCache);
        if (result != Result.Success)
            throw new Exception($"Failed to create pipeline cache with error: {result.ToString()}");
    }


    public unsafe void Dispose()
    {
        _vk.DestroyPipelineCache(_logicalDevice.VkLogicalDevice, VkPipelineCache, null);
        GC.SuppressFinalize(this);
    }
}