using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class Sampler: GHAL.Sampler
{
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;

    public readonly Silk.NET.Vulkan.Sampler VkSampler;
    
    public unsafe Sampler(Vk vk, LogicalDevice logicalDevice)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        _vk.GetPhysicalDeviceProperties(_logicalDevice.PhysicalDevice.VkPhysicalDevice, out var properties);

        SamplerCreateInfo samplerInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Nearest,
            MinFilter = Filter.Nearest,
            AddressModeU = SamplerAddressMode.Repeat,
            AddressModeV = SamplerAddressMode.Repeat,
            AddressModeW = SamplerAddressMode.Repeat,
            AnisotropyEnable = false,
            MaxAnisotropy = properties.Limits.MaxSamplerAnisotropy,
            BorderColor = BorderColor.IntOpaqueBlack,
            UnnormalizedCoordinates = false,
            CompareEnable = false,
            CompareOp = CompareOp.Always,
            MipmapMode = SamplerMipmapMode.Nearest
        };

        var result = _vk.CreateSampler(_logicalDevice.VkLogicalDevice, samplerInfo, null, out VkSampler);
        
        if (result != Result.Success)
            throw new Exception($"Failed to create texture sampler with error: {result.ToString()}");
    }
    
    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}