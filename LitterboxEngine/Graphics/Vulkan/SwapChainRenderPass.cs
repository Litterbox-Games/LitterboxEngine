﻿using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.Vulkan;

// TODO: See about renaming this to just RenderPass

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