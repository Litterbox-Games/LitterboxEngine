using LitterboxEngine.Graphics.GHAL;
using MoreLinq;
using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.Vulkan;

public sealed class ShaderProgram: GHAL.ShaderProgram
{
    public readonly ShaderModule[] ShaderModules;
    public readonly ShaderDescription[] Descriptions;
    
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;
    
    public ShaderProgram(Vk vk, LogicalDevice logicalDevice, ShaderDescription[] descriptions)
    {
        Descriptions = descriptions;
        _vk = vk;
        _logicalDevice = logicalDevice;

        ShaderModules = Descriptions.Select(CreateShaderModule).ToArray();
    }

    private unsafe ShaderModule CreateShaderModule(ShaderDescription description)
    {
        fixed (byte* codePtr = description.Source)
        {

            ShaderModuleCreateInfo createInfo = new()
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)description.Source.Length,
                PCode = (uint*)codePtr
            };
            
            var result = _vk.CreateShaderModule(_logicalDevice.VkLogicalDevice, createInfo, null, out var shaderModule); 
            
            if (result != Result.Success)
                throw new Exception($"Failed to create shader module for shader {description.Path} with error: {result.ToString()}");

            return shaderModule;
        }
    }

    public override unsafe void Dispose()
    {
        ShaderModules.ForEach(module => _vk.DestroyShaderModule(_logicalDevice.VkLogicalDevice, module, null));
    }
}