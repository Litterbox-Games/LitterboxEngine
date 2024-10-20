using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanFrameBuffer: IDisposable
{
    public readonly Framebuffer VkFrameBuffer;

    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;
    
    public unsafe VulkanFrameBuffer(Vk vk, VulkanLogicalDevice logicalDevice, uint width, uint height, VulkanImageView imageView, VulkanRenderPass renderPass)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;

        var attachment = imageView.VkImageView;
        FramebufferCreateInfo framebufferInfo = new()
        {
            SType = StructureType.FramebufferCreateInfo,
            RenderPass = renderPass.VkRenderPass,
            // If we ever need to change attachmentCount, we will need to edit the pipeline and swapchain
            AttachmentCount = 1,
            PAttachments = &attachment,
            Width = width,
            Height = height,
            Layers = 1,
        };
        
        var result = _vk.CreateFramebuffer(_logicalDevice.VkLogicalDevice, in framebufferInfo, null, out VkFrameBuffer);
        if (result != Result.Success)
            throw new Exception($"Failed to create frame buffer with error {result.ToString()}");
    }


    public unsafe void Dispose()
    {
        _vk.DestroyFramebuffer(_logicalDevice.VkLogicalDevice, VkFrameBuffer, null);
        GC.SuppressFinalize(this);
    }
}