using Client.Resource;
using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanTexture : Texture
{
    public readonly VulkanImageView ImageView;
    private readonly VulkanImage _image;

    public unsafe VulkanTexture(Vk vk, VulkanLogicalDevice logicalDevice, VulkanCommandPool commandPool, VulkanQueue queue, uint width, uint height, Span<byte> data) : base(width, height, data.ToArray())
    {
        var size = (ulong)data.Length;
        using var stagingBuffer = new VulkanStagingBuffer(vk, logicalDevice, commandPool, queue, size);

        void* dataPtr;
        vk.MapMemory(logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory, 0, size, 0, &dataPtr);
        data.CopyTo(new Span<byte>(dataPtr, (int)size));
        vk.UnmapMemory(logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory);
        
        _image = new VulkanImage(vk, logicalDevice, width, height, Format.R8G8B8A8Srgb,
            ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit);
        
        _image.TransitionLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal, commandPool, queue);
        stagingBuffer.CopyTo(_image);
        _image.TransitionLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal, commandPool, queue);

        ImageView = new VulkanImageView(vk, logicalDevice, _image.VkImage,
            new VulkanImageView.ImageViewData(Format.R8G8B8A8Srgb, ImageAspectFlags.ColorBit));
    }

    public override void Dispose()
    {
        ImageView.Dispose();
        _image.Dispose();
        GC.SuppressFinalize(this);
    }
}