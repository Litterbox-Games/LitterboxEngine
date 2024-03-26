using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class Texture : IDisposable
{
    private readonly Vk _vk;
    private readonly LogicalDevice _logicalDevice;

    public readonly ImageView ImageView;
    public readonly Image Image;

    public unsafe Texture(Vk vk, LogicalDevice logicalDevice, CommandPool commandPool, Queue queue)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;

        using var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>("Resources/Textures/litterbox_logo.png");

        var imageSize = (uint)(image.Width * image.Height * image.PixelType.BitsPerPixel / 8);
        
        var stagingBufferDescription = new BufferDescription(imageSize, BufferUsage.Transfer);                     
        using var stagingBuffer = new Buffer(_vk, _logicalDevice, stagingBufferDescription, 
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit, commandPool, queue);

        void* dataPtr;
        _vk.MapMemory(_logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory, 0, imageSize, 0, &dataPtr);
        image.CopyPixelDataTo(new Span<byte>(dataPtr, (int)imageSize));
        _vk.UnmapMemory(_logicalDevice.VkLogicalDevice, stagingBuffer.VkBufferMemory);
        
        Image = new Image(_vk, _logicalDevice, (uint)image.Width, (uint)image.Height, Format.R8G8B8A8Srgb,
            ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit);
        
        Image.TransitionLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal, commandPool, queue);
        stagingBuffer.CopyTo(Image);
        Image.TransitionLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal, commandPool, queue);

        ImageView = new ImageView(_vk, _logicalDevice, Image.VkImage,
            new ImageView.ImageViewData(Format.R8G8B8A8Srgb, ImageAspectFlags.ColorBit));
    }


    public void Dispose()
    {
        throw new NotImplementedException();
    }
}