using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class Texture : LitterboxEngine.Graphics.Resources.Texture
{
    public readonly ImageView ImageView;
    private readonly Image _image;

    public  unsafe Texture(Vk vk, LogicalDevice logicalDevice, CommandPool commandPool, Queue queue, uint width, uint height,
        RgbaByte color) : base(width, height)
    {
        var sizeInBytes = (ulong)(width * height * sizeof(RgbaByte));
        var stagingBufferDescription = new BufferDescription(sizeInBytes, BufferUsage.Transfer);                     
        using var stagingBuffer = new Buffer(vk, logicalDevice, stagingBufferDescription, 
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, commandPool, queue);

        void* dataPtr;
        vk.MapMemory(logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory, 0, sizeInBytes, 0, &dataPtr);
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
        var sizeInBytes = (ulong)data.Length;
        
        var stagingBufferDescription = new BufferDescription(sizeInBytes, BufferUsage.Transfer);                     
        using var stagingBuffer = new Buffer(vk, logicalDevice, stagingBufferDescription, 
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, commandPool, queue);

        void* dataPtr;
        vk.MapMemory(logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory, 0, sizeInBytes, 0, &dataPtr);
        data.CopyTo(new Span<byte>(dataPtr, (int)sizeInBytes));
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