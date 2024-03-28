using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class DescriptorSet : ResourceSet
{
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;

    public readonly Silk.NET.Vulkan.DescriptorSet VkDescriptorSet; 
    
    public unsafe DescriptorSet(Vk vk, LogicalDevice logicalDevice, DescriptorPool pool, DescriptorSetLayout layout)
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
                PSetLayouts = layoutsPtr
            };         
            
            var result = _vk.AllocateDescriptorSets(_logicalDevice.VkLogicalDevice, allocateInfo, out VkDescriptorSet);
            if (result != Result.Success)
                throw new Exception($"Failed to allocate descriptor sets with error: {result.ToString()}");
        }
    }

    public override unsafe void Update(uint binding, GHAL.Buffer buffer, uint index = 0)
    {
        var vkBuffer = (buffer as Buffer)!;
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

    public override unsafe void Update(uint binding, GHAL.Sampler sampler, uint index = 0)
    {
        var vkSampler = (sampler as Sampler)!;
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

    public override unsafe void Update(uint binding, Resources.Texture texture, uint index = 0)
    {
        var vkTexture = (texture as Texture)!;
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