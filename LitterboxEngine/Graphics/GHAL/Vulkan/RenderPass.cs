using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class RenderPass: IDisposable
{
    public readonly Silk.NET.Vulkan.RenderPass VkRenderPass;
    
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;
    
    public unsafe RenderPass(Vk vk, LogicalDevice logicalDevice, Format format)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        AttachmentDescription colorAttachment = new()
        {
            Format = format,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr,
        };

        AttachmentReference colorAttachmentRef = new()
        {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal,
        };

        SubpassDescription subpass = new()
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef,
        };

        SubpassDependency subpassDependency = new()
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = 0,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit
        };

        RenderPassCreateInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &colorAttachment,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &subpassDependency
        };

        var result = _vk.CreateRenderPass(_logicalDevice.VkLogicalDevice, renderPassInfo, null, out VkRenderPass);
        if (result != Result.Success)
            throw new Exception($"Failed to create render pass with error: {result.ToString()}");
    }

    public unsafe void Dispose()
    {
        _vk.DestroyRenderPass(_logicalDevice.VkLogicalDevice, VkRenderPass, null);
        GC.SuppressFinalize(this);
    }
}