using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class Texture : LitterboxEngine.Graphics.Resources.Texture
{
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;

    public readonly ImageView ImageView;
    public readonly Image Image;

    // TODO: Can we just pass in a size and span instead maybe?

    public  unsafe Texture(Vk vk, LogicalDevice logicalDevice, CommandPool commandPool, Queue queue, uint width, uint height,
        RgbaByte color) : base(width, height)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;

        var sizeInBytes = (ulong)(width * height * sizeof(RgbaByte));
        var stagingBufferDescription = new BufferDescription(sizeInBytes, BufferUsage.Transfer);                     
        using var stagingBuffer = new Buffer(_vk, _logicalDevice, stagingBufferDescription, 
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, commandPool, queue);

        void* dataPtr;
        _vk.MapMemory(_logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory, 0, sizeInBytes, 0, &dataPtr);
        new Span<RgbaByte>(dataPtr, (int)(width * height)).Fill(color);
        _vk.UnmapMemory(_logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory);
        
        Image = new Image(_vk, _logicalDevice, width, height, Format.R8G8B8A8Srgb,
            ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit);
        
        Image.TransitionLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal, commandPool, queue);
        stagingBuffer.CopyTo(Image);
        Image.TransitionLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal, commandPool, queue);

        ImageView = new ImageView(_vk, _logicalDevice, Image.VkImage,
            new ImageView.ImageViewData(Format.R8G8B8A8Srgb, ImageAspectFlags.ColorBit));
    }
    
    public unsafe Texture(Vk vk, LogicalDevice logicalDevice, CommandPool commandPool, Queue queue, uint width, uint height, Span<byte> data) : base(width, height)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;

        var sizeInBytes = (ulong)data.Length;
        
        var stagingBufferDescription = new BufferDescription(sizeInBytes, BufferUsage.Transfer);                     
        using var stagingBuffer = new Buffer(_vk, _logicalDevice, stagingBufferDescription, 
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, commandPool, queue);

        void* dataPtr;
        _vk.MapMemory(_logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory, 0, sizeInBytes, 0, &dataPtr);
        data.CopyTo(new Span<byte>(dataPtr, (int)sizeInBytes));
        _vk.UnmapMemory(_logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory);
        
        Image = new Image(_vk, _logicalDevice, width, height, Format.R8G8B8A8Srgb,
            ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit);
        
        Image.TransitionLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal, commandPool, queue);
        stagingBuffer.CopyTo(Image);
        Image.TransitionLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal, commandPool, queue);

        ImageView = new ImageView(_vk, _logicalDevice, Image.VkImage,
            new ImageView.ImageViewData(Format.R8G8B8A8Srgb, ImageAspectFlags.ColorBit));
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}