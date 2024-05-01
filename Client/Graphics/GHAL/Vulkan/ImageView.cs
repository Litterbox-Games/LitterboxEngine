using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class ImageView: IDisposable
{
    private readonly Vk _vk;
    public readonly Silk.NET.Vulkan.ImageView VkImageView;
    private readonly LogicalDevice _logicalDevice;
    
    public unsafe ImageView(Vk vk, LogicalDevice logicalDevice, Silk.NET.Vulkan.Image vkImage, ImageViewData imageViewData)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        
        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = vkImage,
            ViewType = imageViewData.ViewType,
            Format = imageViewData.Format,
            SubresourceRange =
            {
                AspectMask = imageViewData.AspectMask,
                BaseMipLevel = 0,
                LevelCount = imageViewData.MipLevels,
                BaseArrayLayer = imageViewData.BaseArrayLayer,
                LayerCount = imageViewData.LayerCount,
            }

        };
        
        var result = _vk.CreateImageView(_logicalDevice.VkLogicalDevice, createInfo, null, out VkImageView);
        if (result != Result.Success)
            throw new Exception($"Failed to create image view with error: {result.ToString()}");
    }

    public unsafe void Dispose()
    {
        _vk.DestroyImageView(_logicalDevice.VkLogicalDevice, VkImageView, null);
        GC.SuppressFinalize(this);
    }
    
    public struct ImageViewData
    {
        public readonly ImageAspectFlags AspectMask;
        public readonly Format Format;
        public readonly uint BaseArrayLayer;
        public readonly uint MipLevels;
        public readonly uint LayerCount;
        public readonly ImageViewType ViewType;

        public ImageViewData(Format format, ImageAspectFlags aspectMask, uint baseArrayLayer = 0, uint mipLevels = 1,
            uint layerCount = 1, ImageViewType viewType = ImageViewType.Type2D)
        {
            Format = format;
            AspectMask = aspectMask;
            BaseArrayLayer = baseArrayLayer;
            MipLevels = mipLevels;
            LayerCount = layerCount;
            ViewType = viewType;
        }

    }
}