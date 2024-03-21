using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class DescriptorPool : IDisposable
{
    public readonly Silk.NET.Vulkan.DescriptorPool VkDescriptorPool;
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;
    
    public unsafe DescriptorPool(Vk vk, LogicalDevice logicalDevice, uint descriptorCount = 100)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;

        // TODO: Add pool sizes for other possible descriptor types (check ResourceKindArray)
        var poolSizes = new DescriptorPoolSize[]
        {
            new()  // UBO Pool Size
            {
                Type = DescriptorType.UniformBuffer,
                DescriptorCount = descriptorCount,
            },
            new() // Sampler Pool Size
            {
                Type = DescriptorType.CombinedImageSampler,
                DescriptorCount = descriptorCount,
            }
        };

        fixed (DescriptorPoolSize* poolSizesPtr = poolSizes)
        {
            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 1,
                PPoolSizes = poolSizesPtr,
                MaxSets = descriptorCount,
            };

            var result = _vk.CreateDescriptorPool(_logicalDevice.VkLogicalDevice, poolInfo, null, out VkDescriptorPool);
            if (result != Result.Success)
                throw new Exception($"Failed to create descriptor pool with error: {result.ToString()}");
        }
    }

    public unsafe void Dispose()
    {
        _vk.DestroyDescriptorPool(_logicalDevice.VkLogicalDevice, VkDescriptorPool, null);    
        GC.SuppressFinalize(this);
    }
}