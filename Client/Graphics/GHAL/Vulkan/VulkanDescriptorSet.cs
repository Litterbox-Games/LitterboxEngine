using Client.Resource;
using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanDescriptorSet : ResourceSet
{
    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;

    public readonly DescriptorSet VkDescriptorSet; 
    
    public unsafe VulkanDescriptorSet(Vk vk, VulkanLogicalDevice logicalDevice, VulkanDescriptorPool pool, VulkanDescriptorSetLayout layout)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        fixed (DescriptorSetLayout* layoutsPtr = new[] { layout.VkDescriptorSetLayout })
        {
            DescriptorSetAllocateInfo allocateInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = pool.VkDescriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = layoutsPtr
            };         
            
            var result = _vk.AllocateDescriptorSets(_logicalDevice.VkLogicalDevice, in allocateInfo, out VkDescriptorSet);
            if (result != Result.Success)
                throw new Exception($"Failed to allocate descriptor sets with error: {result.ToString()}");
        }
    }

    public override unsafe void UpdateStorageBuffer(uint binding, Buffer buffer, uint index = 0)
    {
        var vkBuffer = (buffer as VulkanBuffer)!;
        DescriptorBufferInfo bufferInfo = new()
        {
            Buffer = vkBuffer.VkBuffer,
            Offset = 0,
            Range = vkBuffer.Size
        };

        var descriptorWrite = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = VkDescriptorSet,
            DstBinding = binding,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            DstArrayElement = index,
            PBufferInfo = &bufferInfo,
        };
        
        _vk.UpdateDescriptorSets(_logicalDevice.VkLogicalDevice, 1, &descriptorWrite, 0, null);
    }
    
    public override unsafe void Update(uint binding, Buffer buffer, uint index = 0)
    {
        var vkBuffer = (buffer as VulkanBuffer)!;
        DescriptorBufferInfo bufferInfo = new()
        {
            Buffer = vkBuffer.VkBuffer,
            Offset = 0,
            Range = vkBuffer.Size
        };

        var descriptorWrite = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = VkDescriptorSet,
            DstBinding = binding,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            DstArrayElement = index,
            PBufferInfo = &bufferInfo,
        };
        
        _vk.UpdateDescriptorSets(_logicalDevice.VkLogicalDevice, 1, &descriptorWrite, 0, null);
    }

    public override unsafe void Update(uint binding, Sampler sampler, uint index = 0)
    {
        var vkSampler = (sampler as VulkanSampler)!;
        DescriptorImageInfo samplerInfo = new()
        {
          Sampler = vkSampler.VkSampler
        };

        var descriptorWrite = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = VkDescriptorSet,
            DstBinding = binding,
            DescriptorType = DescriptorType.Sampler,
            DescriptorCount = 1,
            DstArrayElement = index,
            PImageInfo = &samplerInfo,
        };
        
        _vk.UpdateDescriptorSets(_logicalDevice.VkLogicalDevice, 1, &descriptorWrite, 0, null);
    }

    public override unsafe void Update(uint binding, Texture texture, uint index = 0)
    {
        var vkTexture = (texture as VulkanTexture)!;
        DescriptorImageInfo imageInfo = new()
        {
            ImageView = vkTexture.ImageView.VkImageView,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal
        };
        
        var descriptorWrite = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = VkDescriptorSet,
            DstBinding = binding,
            DescriptorType = DescriptorType.SampledImage,
            DescriptorCount = 1,
            DstArrayElement = index,
            PImageInfo = &imageInfo,
        };
        
        _vk.UpdateDescriptorSets(_logicalDevice.VkLogicalDevice, 1, &descriptorWrite, 0, null);
    }
}