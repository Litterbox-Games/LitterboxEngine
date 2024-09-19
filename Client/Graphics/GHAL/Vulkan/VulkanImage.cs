using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanImage: IDisposable
{
    private readonly Vk _vk;
    private readonly VulkanLogicalDevice _logicalDevice;

    public readonly Image VkImage;
    private readonly DeviceMemory _vkMemory;

    public readonly uint Width;
    public readonly uint Height;

    public unsafe VulkanImage(Vk vk, VulkanLogicalDevice logicalDevice, uint width, uint height, Format format, ImageUsageFlags usage)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;

        Width = width;
        Height = height;
        
        
        ImageCreateInfo imageInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent =
            {
                Width = width,
                Height = height,
                Depth = 1,
            },
            MipLevels = 1,
            ArrayLayers = 1,
            Format = format,
            Tiling = ImageTiling.Optimal,
            InitialLayout = ImageLayout.Undefined,
            Usage = usage,
            Samples = SampleCountFlags.Count1Bit,
            SharingMode = SharingMode.Exclusive,
        };

        var result = _vk.CreateImage(_logicalDevice.VkLogicalDevice, imageInfo, null, out VkImage);

        if (result != Result.Success)
            throw new Exception($"Failed to create image with error {result.ToString()}");

        _vk.GetImageMemoryRequirements(_logicalDevice.VkLogicalDevice, VkImage, out var memRequirements);

        MemoryAllocateInfo allocInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = MemoryTypeFromProperties(memRequirements.MemoryTypeBits, 0),
        };

        result = _vk.AllocateMemory(_logicalDevice.VkLogicalDevice, allocInfo, null, out _vkMemory);

        if (result != Result.Success)
            throw new Exception($"Failed to allocate image memory with error: {result.ToString()}");

        _vk.BindImageMemory(_logicalDevice.VkLogicalDevice, VkImage, _vkMemory, 0);
    }

    public unsafe void TransitionLayout(ImageLayout oldLayout, ImageLayout newLayout, VulkanCommandPool commandPool, VulkanQueue queue)
    {
        using var commandBuffer = new VulkanCommandBuffer(_vk, commandPool, true, true);
        commandBuffer.BeginRecording();

        ImageMemoryBarrier barrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = VkImage,
            SubresourceRange =
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1,
            }
        };

        PipelineStageFlags sourceStage;
        PipelineStageFlags destinationStage;

        if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
        {
            barrier.SrcAccessMask = 0;
            barrier.DstAccessMask = AccessFlags.TransferWriteBit;

            sourceStage = PipelineStageFlags.TopOfPipeBit;
            destinationStage = PipelineStageFlags.TransferBit;
        }
        else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;

            sourceStage = PipelineStageFlags.TransferBit;
            destinationStage = PipelineStageFlags.FragmentShaderBit;
        }
        else
        {
            throw new Exception($"Unsupported layout transition: old: ${oldLayout} new: ${newLayout}");
        }

        _vk.CmdPipelineBarrier(commandBuffer.VkCommandBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1, barrier);

        commandBuffer.EndRecording();
        using var fence = new VulkanFence(_vk, _logicalDevice, true);
        fence.Reset();
        queue.Submit(commandBuffer, null, fence);
        fence.Wait();
    }
    
    private uint MemoryTypeFromProperties(uint typeFilter, MemoryPropertyFlags properties)
    {
        _vk.GetPhysicalDeviceMemoryProperties(_logicalDevice.PhysicalDevice.VkPhysicalDevice, out var memProperties);

        for (var i = 0; i < memProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & 1) == 1 && (memProperties.MemoryTypes[i].PropertyFlags & properties) == properties)
                return (uint) i;

            typeFilter >>= 1;
        }

        throw new Exception("Failed to find suitable memory type");
    }

    public unsafe void Dispose()
    {
        _vk.DestroyImage(_logicalDevice.VkLogicalDevice, VkImage, null);
        _vk.FreeMemory(_logicalDevice.VkLogicalDevice, _vkMemory, null);
        GC.SuppressFinalize(this);
    }
}