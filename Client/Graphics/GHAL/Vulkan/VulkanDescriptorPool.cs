﻿using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanDescriptorPool : IDisposable
{
    public readonly DescriptorPool VkDescriptorPool;
    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;
    
    public unsafe VulkanDescriptorPool(Vk vk, VulkanLogicalDevice logicalDevice, uint descriptorCount = 100)
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
                Type = DescriptorType.Sampler,
                DescriptorCount = descriptorCount,
            },
            new() // Sampler Pool Size
            {
                Type = DescriptorType.SampledImage,
                DescriptorCount = descriptorCount,
            },
            new() // Storage Buffer Pool Size
            {
                Type = DescriptorType.StorageBuffer,
                DescriptorCount = descriptorCount,
            }
        };

        fixed (DescriptorPoolSize* poolSizesPtr = poolSizes)
        {
            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = poolSizesPtr,
                MaxSets = descriptorCount,
            };

            var result = _vk.CreateDescriptorPool(_logicalDevice.VkLogicalDevice, in poolInfo, null, out VkDescriptorPool);
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