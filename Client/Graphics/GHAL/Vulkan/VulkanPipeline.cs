using MoreLinq;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanPipeline: Pipeline
{
    public readonly Silk.NET.Vulkan.Pipeline VkPipeline;
    public readonly PipelineLayout VkPipelineLayout;
    
    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;
    private readonly VulkanDescriptorSetLayout[] _descriptorSetLayouts;
    
    // TODO: Enabling scissor test
    // TODO: Blending support from pipeline description
    public unsafe VulkanPipeline(Vk vk, VulkanLogicalDevice logicalDevice, VulkanRenderPass renderPass, VulkanPipelineCache cache, PipelineDescription description)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;

        var shaderProgram = (description.ShaderSet.ShaderProgram as VulkanShaderProgram)!;
        var shaderStages = CreateShaderStages(shaderProgram);

        _descriptorSetLayouts = description.ResourceLayouts.Select(l => (l as VulkanDescriptorSetLayout)!).ToArray();
        var descriptorSetLayouts = _descriptorSetLayouts.Select(l => l.VkDescriptorSetLayout).ToArray();
        
        var attributeDescriptions = description.ShaderSet.VertexLayout.ElementDescriptions
            .Select(element => new VertexInputAttributeDescription
            {
                Binding = element.Binding,
                Location = element.Location,
                Format = ToVkElementFormat(element.Format),
                Offset = element.Offset
            }).ToArray();

        fixed (PipelineShaderStageCreateInfo* shaderStagesPtr = shaderStages)
        fixed (DescriptorSetLayout* descriptorSetLayoutsPtr = descriptorSetLayouts)
        fixed (VertexInputAttributeDescription* attributeDescriptionsPtr = attributeDescriptions)
        {
            PipelineViewportStateCreateInfo viewportState = new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount = 1
            };

            var bindingDescription = new VertexInputBindingDescription
            {
                Binding = description.ShaderSet.VertexLayout.Binding,
                Stride = description.ShaderSet.VertexLayout.Stride,
                InputRate = VertexInputRate.Vertex
            };
            
            var vertexInputState = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                // VertexBindingDescriptionCount = 1,
                VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length,
                // PVertexBindingDescriptions = &bindingDescription,
                PVertexAttributeDescriptions = attributeDescriptionsPtr,
            };
            
            var inputAssemblyState = CreateInputAssemblyState(description.PrimitiveTopology);
            
            var rasterizationState = CreateRasterizationState(description.RasterizationState);

            PipelineMultisampleStateCreateInfo multisampleState = new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = SampleCountFlags.Count1Bit // Get this from PipelineDescription?
            };
            
            PipelineColorBlendAttachmentState colorBlendAttachment = new()
            {
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
                BlendEnable = false, // Get this from PipelineDescription?
                // TODO: set blending modes information form PipelineDescription here as well
            };
            
            PipelineColorBlendStateCreateInfo colorBlendState = new()
            {
                // TODO: Same as above, should be created based on the pipeline description
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable = false,
                LogicOp = LogicOp.Copy,
                AttachmentCount = 1,
                PAttachments = &colorBlendAttachment,
            };
            
            colorBlendState.BlendConstants[0] = 0;
            colorBlendState.BlendConstants[1] = 0;
            colorBlendState.BlendConstants[2] = 0;
            colorBlendState.BlendConstants[3] = 0;
            
            var dynamicStates = stackalloc[]
            {
                DynamicState.Viewport,
                DynamicState.Scissor
            };

            PipelineDynamicStateCreateInfo dynamicState = new()
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates = dynamicStates
            };
            
            PipelineLayoutCreateInfo pipelineLayoutInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = (uint)descriptorSetLayouts.Length,
                PSetLayouts = descriptorSetLayoutsPtr
            };
            
            var result = _vk.CreatePipelineLayout(_logicalDevice.VkLogicalDevice, pipelineLayoutInfo, null, out VkPipelineLayout);
            if (result != Result.Success)
                throw new Exception($"Failed to create graphics pipeline layout with error: {result.ToString()}");
            
            GraphicsPipelineCreateInfo pipelineInfo = new()
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                
                StageCount = (uint)shaderStages.Length,
                PStages = shaderStagesPtr,
                
                PVertexInputState = &vertexInputState,
                PInputAssemblyState = &inputAssemblyState,
                PViewportState = &viewportState,
                PRasterizationState = &rasterizationState,
                PMultisampleState = &multisampleState,
                PColorBlendState = &colorBlendState,
                PDynamicState = &dynamicState,
                
                Layout = VkPipelineLayout,
                
                RenderPass = renderPass.VkRenderPass,
                Subpass = 0,
                
                BasePipelineHandle = default
            };
            
            result = _vk.CreateGraphicsPipelines(_logicalDevice.VkLogicalDevice, cache.VkPipelineCache, 1, pipelineInfo, null, out VkPipeline);
            if (result != Result.Success)
                throw new Exception($"Failed to create graphics pipeline with error: {result.ToString()}");
            
            shaderStages.ForEach(shaderStage =>  SilkMarshal.Free((nint)shaderStage.PName));
        }
    }

    #region VertexInputState
    private static Format ToVkElementFormat(VertexElementFormat format)
    {
        return format switch
        {
            VertexElementFormat.Float => Format.R32Sfloat,
            VertexElementFormat.Float2 => Format.R32G32Sfloat,
            VertexElementFormat.Float3 => Format.R32G32B32Sfloat,
            VertexElementFormat.Float4 => Format.R32G32B32A32Sfloat,
            VertexElementFormat.Int => Format.R32Sint,
            _ => throw new NotImplementedException($"Cannot convert VertexElementFormat \"{format}\" to Format")
        };
    } 
    #endregion
    
    #region InputAssemblyState
    private static PipelineInputAssemblyStateCreateInfo CreateInputAssemblyState(PrimitiveTopology primitiveTopology)
    {
        return new PipelineInputAssemblyStateCreateInfo
        {
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = ToVkPrimitiveTopology(primitiveTopology),
            PrimitiveRestartEnable = false
        };
    }

    private static Silk.NET.Vulkan.PrimitiveTopology ToVkPrimitiveTopology(PrimitiveTopology primitiveTopology)
    {
        return primitiveTopology switch
        {
            PrimitiveTopology.TriangleList => Silk.NET.Vulkan.PrimitiveTopology.TriangleList,
            PrimitiveTopology.LineList => Silk.NET.Vulkan.PrimitiveTopology.LineList,
            PrimitiveTopology.LineStrip => Silk.NET.Vulkan.PrimitiveTopology.LineStrip,
            PrimitiveTopology.PatchList => Silk.NET.Vulkan.PrimitiveTopology.PatchList, 
            PrimitiveTopology.PointList => Silk.NET.Vulkan.PrimitiveTopology.PointList,
            PrimitiveTopology.TriangleFan => Silk.NET.Vulkan.PrimitiveTopology.TriangleFan,
            PrimitiveTopology.TriangleStrip => Silk.NET.Vulkan.PrimitiveTopology.TriangleStrip,
            PrimitiveTopology.LineListWithAdjacency => Silk.NET.Vulkan.PrimitiveTopology.LineListWithAdjacency,
            PrimitiveTopology.LineStripWithAdjacency => Silk.NET.Vulkan.PrimitiveTopology.LineStripWithAdjacency,
            PrimitiveTopology.TriangleListWithAdjacency => Silk.NET.Vulkan.PrimitiveTopology.TriangleListWithAdjacency,
            PrimitiveTopology.TriangleStripWithAdjacency => Silk.NET.Vulkan.PrimitiveTopology.TriangleStripWithAdjacency,
            _ => throw new NotImplementedException($"Cannot convert PrimitiveTopology \"{primitiveTopology}\" to VkPrimitiveTopology")
        };
    }
    #endregion
    
    #region RasterizationState
    private static PipelineRasterizationStateCreateInfo CreateRasterizationState(RasterizationStateDescription description)
    {
        return new PipelineRasterizationStateCreateInfo
        {
            SType = StructureType.PipelineRasterizationStateCreateInfo,
            PolygonMode = ToVkPolygonMode(description.FillMode),
            CullMode = ToVkCullMode(description.CullMode),
            DepthClampEnable = !description.EnableDepthTest, 
            LineWidth = description.LineWidth,
            FrontFace = description.FrontFace == FrontFace.ClockWise ? 
                Silk.NET.Vulkan.FrontFace.Clockwise : Silk.NET.Vulkan.FrontFace.CounterClockwise 
        };
    }

    private static PolygonMode ToVkPolygonMode(FillMode fillMode)
    {
        return fillMode switch
        {
            FillMode.Solid => PolygonMode.Fill,
            FillMode.Line => PolygonMode.Line,
            FillMode.Point => PolygonMode.Point,
            _ => throw new NotImplementedException($"Cannot convert FillMode \"{fillMode}\" to PolygonMode")
        };
    }

    private static CullModeFlags ToVkCullMode(CullMode cullMode)
    {
        return cullMode switch
        {
            CullMode.None => CullModeFlags.None,
            CullMode.Front => CullModeFlags.FrontBit,
            CullMode.Back => CullModeFlags.BackBit,
            CullMode.FrontAndBack => CullModeFlags.FrontAndBack,
            _ => throw new NotImplementedException($"Cannot convert CullMode \"{cullMode}\" to CullModeFlags")
        };
    }
    #endregion
    
    #region ShaderStage
    private static PipelineShaderStageCreateInfo[] CreateShaderStages(VulkanShaderProgram shaderProgram)
    {
        return shaderProgram.ShaderModules
            .Zip(shaderProgram.Descriptions)
            .Select(tuple => CreateShaderStage(tuple.First, tuple.Second))
            .ToArray();
    }
    
    private static unsafe PipelineShaderStageCreateInfo CreateShaderStage(ShaderModule module, ShaderDescription description)
    {
        return new PipelineShaderStageCreateInfo
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageToShaderStageFlag(description.ShaderStage),
            Module = module,
            PName = (byte*) SilkMarshal.StringToPtr(description.EntryPoint)
        };
    }

    private static ShaderStageFlags ShaderStageToShaderStageFlag(ShaderStages shaderStage)
    {
        return shaderStage switch
        {
            ShaderStages.Fragment => ShaderStageFlags.FragmentBit,
            ShaderStages.Vertex => ShaderStageFlags.VertexBit,
            _ => throw new NotImplementedException($"Cannot convert ShaderStage \"{shaderStage}\" to ShaderStageFlag")
        };
    }
    #endregion

    public override unsafe void Dispose()
    {
        _descriptorSetLayouts.ForEach(l => l.Dispose());   
        _vk.DestroyPipeline(_logicalDevice.VkLogicalDevice, VkPipeline, null);
        _vk.DestroyPipelineLayout(_logicalDevice.VkLogicalDevice, VkPipelineLayout, null);
        GC.SuppressFinalize(this);
    }
}