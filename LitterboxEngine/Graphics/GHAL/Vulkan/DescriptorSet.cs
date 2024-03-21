using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class DescriptorSet : ResourceSet
{
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;

    public readonly Silk.NET.Vulkan.DescriptorSet VkDescriptorSet; 
    
    public unsafe DescriptorSet(Vk vk, LogicalDevice logicalDevice, DescriptorPool pool, Buffer buffer, DescriptorSetLayout layout, uint binding, DescriptorType descriptorType)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        fixed (Silk.NET.Vulkan.DescriptorSetLayout* layoutsPtr = new[] { layout.VkDescriptorSetLayout })
        {
            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = pool.VkDescriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = layoutsPtr,
            };         
            
            var result = _vk.AllocateDescriptorSets(_logicalDevice.VkLogicalDevice, allocateInfo, out VkDescriptorSet);
            if (result != Result.Success)
                throw new Exception($"Failed to allocate descriptor sets with error: {result.ToString()}");
        
            DescriptorBufferInfo bufferInfo = new()
            {
                Buffer = buffer.VkBuffer,
                Offset = 0,
                Range = buffer.Size
            };

            var descriptorWrite = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = VkDescriptorSet,
                DstBinding = binding,
                DescriptorType = descriptorType,
                DescriptorCount = 1,
                PBufferInfo = &bufferInfo,
            };
            
            _vk.UpdateDescriptorSets(_logicalDevice.VkLogicalDevice, 1, &descriptorWrite, 0, null);
        }
    }
}