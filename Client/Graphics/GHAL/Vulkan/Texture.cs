using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class Texture : Resources.Texture
{
    public readonly ImageView ImageView;
    private readonly Image _image;

    public  unsafe Texture(Vk vk, LogicalDevice logicalDevice, CommandPool commandPool, Queue queue, uint width, uint height,
        RgbaByte color) : base(width, height)
    {
        var size = (ulong)(width * height * sizeof(RgbaByte));
        using var stagingBuffer = new StagingBuffer(vk, logicalDevice, commandPool, queue, size);
        
        void* dataPtr;
        vk.MapMemory(logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory, 0, size, 0, &dataPtr);
        new Span<RgbaByte>(dataPtr, (int)(width * height)).Fill(color);
        vk.UnmapMemory(logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory);
        
        _image = new Image(vk, logicalDevice, width, height, Format.R8G8B8A8Srgb,
            ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit);
        
        _image.TransitionLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal, commandPool, queue);
        stagingBuffer.CopyTo(_image);
        _image.TransitionLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal, commandPool, queue);

        ImageView = new ImageView(vk, logicalDevice, _image.VkImage,
            new ImageView.ImageViewData(Format.R8G8B8A8Srgb, ImageAspectFlags.ColorBit));
    }
    
    public unsafe Texture(Vk vk, LogicalDevice logicalDevice, CommandPool commandPool, Queue queue, uint width, uint height, Span<byte> data) : base(width, height)
    {
        var size = (ulong)data.Length;
        using var stagingBuffer = new StagingBuffer(vk, logicalDevice, commandPool, queue, size);

        void* dataPtr;
        vk.MapMemory(logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory, 0, size, 0, &dataPtr);
        data.CopyTo(new Span<byte>(dataPtr, (int)size));
        vk.UnmapMemory(logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory);
        
        _image = new Image(vk, logicalDevice, width, height, Format.R8G8B8A8Srgb,
            ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit);
        
        _image.TransitionLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal, commandPool, queue);
        stagingBuffer.CopyTo(_image);
        _image.TransitionLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal, commandPool, queue);

        ImageView = new ImageView(vk, logicalDevice, _image.VkImage,
            new ImageView.ImageViewData(Format.R8G8B8A8Srgb, ImageAspectFlags.ColorBit));
    }

    public override void Dispose()
    {
        ImageView.Dispose();
        _image.Dispose();
        GC.SuppressFinalize(this);
    }
}