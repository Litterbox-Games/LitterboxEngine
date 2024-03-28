using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class FrameBuffer: IDisposable
{
    public readonly Framebuffer VkFrameBuffer;

    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;
    
    public unsafe FrameBuffer(Vk vk, LogicalDevice logicalDevice, uint width, uint height, ImageView imageView, RenderPass renderPass)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;

        var attachment = imageView.VkImageView;
        FramebufferCreateInfo framebufferInfo = new()
        {
            SType = StructureType.FramebufferCreateInfo,
            RenderPass = renderPass.VkRenderPass,
            AttachmentCount = 1,
            PAttachments = &attachment,
            Width = width,
            Height = height,
            Layers = 1,
        };
        
        var result = _vk.CreateFramebuffer(_logicalDevice.VkLogicalDevice, framebufferInfo, null, out VkFrameBuffer);
        if (result != Result.Success)
            throw new Exception($"Failed to create frame buffer with error {result.ToString()}");
    }


    public unsafe void Dispose()
    {
        _vk.DestroyFramebuffer(_logicalDevice.VkLogicalDevice, VkFrameBuffer, null);
        GC.SuppressFinalize(this);
    }
}