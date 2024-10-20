using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanSampler: Sampler
{
    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;

    public readonly Silk.NET.Vulkan.Sampler VkSampler;
    
    public unsafe VulkanSampler(Vk vk, VulkanLogicalDevice logicalDevice)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        _vk.GetPhysicalDeviceProperties(_logicalDevice.PhysicalDevice.VkPhysicalDevice, out var properties);

        SamplerCreateInfo samplerInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Nearest,
            MinFilter = Filter.Nearest,
            AddressModeU = SamplerAddressMode.ClampToEdge,
            AddressModeV = SamplerAddressMode.ClampToEdge,
            AddressModeW = SamplerAddressMode.ClampToEdge,
            AnisotropyEnable = false,
            MaxAnisotropy = properties.Limits.MaxSamplerAnisotropy,
            BorderColor = BorderColor.IntTransparentBlack,
            UnnormalizedCoordinates = false,
            CompareEnable = false,
            CompareOp = CompareOp.Always,
            MipmapMode = SamplerMipmapMode.Nearest
        };

        var result = _vk.CreateSampler(_logicalDevice.VkLogicalDevice, in samplerInfo, null, out VkSampler);
        
        if (result != Result.Success)
            throw new Exception($"Failed to create texture sampler with error: {result.ToString()}");
    }
    
    public override unsafe void Dispose()
    {
        _vk.DestroySampler(_logicalDevice.VkLogicalDevice, VkSampler, null);
        GC.SuppressFinalize(this);
    }
}