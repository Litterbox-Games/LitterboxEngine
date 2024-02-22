using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.Vulkan;

public class SwapChainRenderPass: IDisposable
{
    public readonly RenderPass VkRenderPass;
    
    private readonly Vk _vk;
    private readonly SwapChain _swapChain;
    
    public unsafe SwapChainRenderPass(Vk vk, SwapChain swapChain)
    {
        _vk = vk;
        _swapChain = swapChain;
        
        AttachmentDescription colorAttachment = new()
        {
            Format = _swapChain.SurfaceFormat.Format,
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

        RenderPassCreateInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &colorAttachment,
            SubpassCount = 1,
            PSubpasses = &subpass,
        };

        var result = _vk.CreateRenderPass(_swapChain.LogicalDevice.VkLogicalDevice, renderPassInfo, null, out VkRenderPass);
        if (result != Result.Success)
            throw new Exception($"Failed to create render pass with error: {result.ToString()}");
    }

    public unsafe void Dispose()
    {
        _vk.DestroyRenderPass(_swapChain.LogicalDevice.VkLogicalDevice, VkRenderPass, null);
        GC.SuppressFinalize(this);
    }
}