using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class DescriptorSetLayout : ResourceLayout, IDisposable
{
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;

    public readonly Silk.NET.Vulkan.DescriptorSetLayout VkDescriptorSetLayout;
    
    public unsafe DescriptorSetLayout(Vk vk, LogicalDevice logicalDevice, ResourceLayoutDescription description)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        var bindings = description.Elements
            .Select((e, i) => new DescriptorSetLayoutBinding {
                Binding = (uint)i,
                DescriptorCount = e.ArraySize,
                DescriptorType = DescriptorTypeFromResourceKind(e.Kind),
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlagsFromShaderStages(e.Stages)
            }).ToArray();

        fixed (DescriptorSetLayoutBinding* bindingsPtr = bindings)
        {
            DescriptorSetLayoutCreateInfo layoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint)bindings.Length,
                PBindings = bindingsPtr,
            };

            var result = _vk.CreateDescriptorSetLayout(_logicalDevice.VkLogicalDevice, layoutInfo, null, out VkDescriptorSetLayout);
            if (result != Result.Success)
                throw new Exception($"Failed to create descriptor set layout with error: {result.ToString()}");
        }
    }

    private static ShaderStageFlags ShaderStageFlagsFromShaderStages(ShaderStages stages)
    {
        return stages switch
        {
            ShaderStages.Fragment => ShaderStageFlags.FragmentBit,
            ShaderStages.Vertex => ShaderStageFlags.VertexBit,
            _ => throw new ArgumentOutOfRangeException(nameof(stages), stages, null)
        };
    }

    private static DescriptorType DescriptorTypeFromResourceKind(ResourceKind kind)
    {
        return kind switch
        {
          ResourceKind.Sampler => DescriptorType.Sampler,
          ResourceKind.TextureReadOnly => DescriptorType.SampledImage,
          ResourceKind.UniformBuffer => DescriptorType.UniformBuffer,
          _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public unsafe void Dispose()
    {
        _vk.DestroyDescriptorSetLayout(_logicalDevice.VkLogicalDevice, VkDescriptorSetLayout, null);
        GC.SuppressFinalize(this);
    }
}