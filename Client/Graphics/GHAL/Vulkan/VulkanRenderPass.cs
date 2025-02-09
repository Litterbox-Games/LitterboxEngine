﻿using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanRenderPass: IDisposable
{
    public readonly RenderPass VkRenderPass;
    
    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;
    
    public unsafe VulkanRenderPass(Vk vk, VulkanLogicalDevice logicalDevice, Format format)
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
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.ColorAttachmentReadBit
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

        var result = _vk.CreateRenderPass(_logicalDevice.VkLogicalDevice, in renderPassInfo, null, out VkRenderPass);
        if (result != Result.Success)
            throw new Exception($"Failed to create render pass with error: {result.ToString()}");
    }

    public unsafe void Dispose()
    {
        _vk.DestroyRenderPass(_logicalDevice.VkLogicalDevice, VkRenderPass, null);
        GC.SuppressFinalize(this);
    }
}