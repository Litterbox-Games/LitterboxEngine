using Client.Graphics.Input;
using MoreLinq;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanSwapChain: IDisposable
{
    public Extent2D Extent;
    private  SwapchainKHR _vkSwapChain;
    private readonly VulkanLogicalDevice _logicalDevice;
    private readonly VulkanSurface _surface;
    private readonly bool _vsyncEnabled;
    private readonly Vk _vk;
    private readonly VulkanCommandPool _commandPool;
    public readonly int ImageCount;
    private readonly VulkanQueue[]? _concurrentQueues;
    private readonly uint[]? _concurrentFamilyIndices;
    private readonly VulkanRenderPass _renderPass;
    private readonly WindowService _windowService;
    
    private KhrSwapchain _khrSwapChain = null!;
    private VulkanImageView[] _imageViews = null!;
    private SwapChainSyncSemaphores[] _syncSemaphores = null!;
    private VulkanFrameBuffer[] _frameBuffers = null!;
    private VulkanFence[] _fences = null!;
    private VulkanCommandBuffer[] _commandBuffers = null!;

    public VulkanFrameBuffer CurrentFrameBuffer => _frameBuffers[_currentFrame];
    public VulkanCommandBuffer CurrentCommandBuffer => _commandBuffers[_currentFrame];
    public Format Format => _surface.Format.Format;

    private uint _currentFrame;

    public VulkanSwapChain(Vk vk, VulkanLogicalDevice logicalDevice, VulkanSurface surface, VulkanRenderPass renderPass, VulkanCommandPool commandPool, WindowService windowService, int requestedImages, bool vsyncEnabled,
        VulkanQueue presentQueue, VulkanQueue[]? concurrentQueues)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        _surface = surface;
        _commandPool = commandPool;
        _vsyncEnabled = vsyncEnabled;
        _windowService = windowService;
        _renderPass = renderPass;
        _concurrentQueues = concurrentQueues;
        
        var physicalDevice = _logicalDevice.PhysicalDevice;
        surface.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice.VkPhysicalDevice, 
            surface.VkSurface, out var surfaceCapabilities);

        ImageCount = CalculateImageCount(surfaceCapabilities, requestedImages);

        _concurrentFamilyIndices = concurrentQueues?
            .Select(queue => queue.QueueFamilyIndex)
            .Where(queueFamilyIndex => queueFamilyIndex != presentQueue.QueueFamilyIndex)
            .ToArray();
        
        CreateVulkanObjects();
    }

    public void AcquireNextImage()
    {
        uint imageIndex = 0;
        var result = _khrSwapChain.AcquireNextImage(_logicalDevice.VkLogicalDevice, _vkSwapChain, ulong.MaxValue, 
            _syncSemaphores[_currentFrame].ImageAcquisition.VkSemaphore, default, ref imageIndex);

        _currentFrame = imageIndex;

        if (result == Result.ErrorOutOfDateKhr)
        {
            Recreate();
            return;
        }
        
        if (result != Result.Success && result != Result.SuboptimalKhr)
            throw new Exception($"Failed to acquire swap chain image with error: ${result.ToString()}");
    }

    public unsafe void PresentImage(VulkanQueue queue)
    {
        var imageIndex = _currentFrame;
        var signalSemaphores = stackalloc[] { _syncSemaphores[_currentFrame].RenderComplete.VkSemaphore };
        var swapChains = stackalloc[] { _vkSwapChain };
        PresentInfoKHR presentInfo = new()
        {
            SType = StructureType.PresentInfoKhr,

            WaitSemaphoreCount = 1,
            PWaitSemaphores = signalSemaphores,

            SwapchainCount = 1,
            PSwapchains = swapChains,
            
            PImageIndices = &imageIndex
        };

        var result = _khrSwapChain.QueuePresent(queue.VkQueue, presentInfo);
        
        _currentFrame = (_currentFrame + 1) % (uint)_imageViews.Length;

        if (result == Result.ErrorOutOfDateKhr)
        {
            Recreate();
            return;
        }
        
        if (result != Result.Success && result != Result.SuboptimalKhr)
            throw new Exception($"Failed to present KHR with error: ${result.ToString()}");
    }
    
    public void WaitForFence()
    {
        _fences[_currentFrame].Wait();
    }
    
    public void Recreate()
    {
        _logicalDevice.WaitIdle();
        _concurrentQueues?.ForEach(x => x.WaitIdle());
        DisposeVulkanObjects();
        CreateVulkanObjects();
    }

    private unsafe void CreateVulkanObjects()
    {
        _currentFrame = 0;
        
        var physicalDevice = _logicalDevice.PhysicalDevice;
        var instance = physicalDevice.Instance;
        _surface.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice.VkPhysicalDevice, _surface.VkSurface,
            out var surfaceCapabilities);

        Extent = CalculateSwapChainExtent(_windowService, surfaceCapabilities);
        
        var swapChainCreateInfo = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface.VkSurface,
            MinImageCount = (uint)ImageCount,
            ImageFormat = Format,
            ImageColorSpace = _surface.Format.ColorSpace,
            ImageExtent = Extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = surfaceCapabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            Clipped = true,
            PresentMode = _vsyncEnabled ? PresentModeKHR.FifoKhr : PresentModeKHR.ImmediateKhr
        };

        if (!_vk.TryGetDeviceExtension(instance.VkInstance, _logicalDevice.VkLogicalDevice, out _khrSwapChain))
            throw new Exception("VK_KHR_swapchain extension was not found or could not be loaded");

        if (_concurrentFamilyIndices is {Length: > 0})
        {
            fixed (uint* concurrentFamilyIndicesPtr = _concurrentFamilyIndices)
            {
                swapChainCreateInfo.ImageSharingMode = SharingMode.Concurrent;
                swapChainCreateInfo.QueueFamilyIndexCount = (uint)_concurrentFamilyIndices.Length;
                swapChainCreateInfo.PQueueFamilyIndices = concurrentFamilyIndicesPtr;    
            }
        }
        
        var result = _khrSwapChain.CreateSwapchain(_logicalDevice.VkLogicalDevice, swapChainCreateInfo, null, out _vkSwapChain);
        if (result != Result.Success)
            throw new Exception($"Failed to create swap chain with error: {result.ToString()}.");
        
        _imageViews = CreateImageViews(_vk, _logicalDevice, _khrSwapChain, _vkSwapChain, _surface.Format.Format);
        _syncSemaphores = _imageViews.Select(_ => new SwapChainSyncSemaphores(_vk, _logicalDevice)).ToArray();
        _frameBuffers = _imageViews.Select(imageView => new VulkanFrameBuffer(_vk, _logicalDevice, Extent.Width, Extent.Height, imageView, _renderPass)).ToArray();
        _fences = _imageViews.Select(_ => new VulkanFence(_vk, _logicalDevice, true)).ToArray();
        _commandBuffers = _imageViews.Select(_ => new VulkanCommandBuffer(_vk, _commandPool, true, false)).ToArray();
    }
    
    private static int CalculateImageCount(SurfaceCapabilitiesKHR surfaceCapabilities, int requestedImages)
    {
        var maxImages = (int)surfaceCapabilities.MaxImageCount;
        var minImages = (int)surfaceCapabilities.MinImageCount;
        var result = minImages;
        if (maxImages != 0) {
            result = Math.Min(requestedImages, maxImages);
        }
        result = Math.Max(result, minImages);

        return result;
    }

    private static Extent2D CalculateSwapChainExtent(WindowService windowService, SurfaceCapabilitiesKHR surfaceCapabilities)
    {
        if (surfaceCapabilities.CurrentExtent.Width != uint.MaxValue) return surfaceCapabilities.CurrentExtent;
        
        // Surface size undefined. Set to the window size if within bounds
        var width = Math.Min(windowService.Width, (int)surfaceCapabilities.MaxImageExtent.Width);
        width = Math.Max(width, (int)surfaceCapabilities.MinImageExtent.Width);

        var height = Math.Min(windowService.Height, (int)surfaceCapabilities.MaxImageExtent.Height);
        height = Math.Max(height, (int)surfaceCapabilities.MinImageExtent.Height);
            
        
        return new Extent2D
            { Width = (uint)width, Height = (uint)height };
    }

    private static unsafe VulkanImageView[] CreateImageViews(Vk vk, VulkanLogicalDevice logicalDevice, KhrSwapchain khrSwapchain, SwapchainKHR swapchainKhr, Format format)
    {
        uint imageCount = 0;
        var result = khrSwapchain.GetSwapchainImages(logicalDevice.VkLogicalDevice, swapchainKhr, ref imageCount, null);
        
        if (result != Result.Success)
            throw new Exception($"Failed to get surface images with error: {result.ToString()}.");

        if (imageCount == 0)
            throw new Exception($"No surface images were found");
        
        Span<Image> swapChainImages = new Image[imageCount];

        result = khrSwapchain.GetSwapchainImages(logicalDevice.VkLogicalDevice, swapchainKhr, &imageCount, swapChainImages);

        if (result != Result.Success)
            throw new Exception($"Failed to get surface images with error: {result.ToString()}.");

        var imageViews = new VulkanImageView[imageCount];
        var imageViewData = new VulkanImageView.ImageViewData(format, ImageAspectFlags.ColorBit);

        for (var i = 0; i < imageCount; i++)
            imageViews[i] = new VulkanImageView(vk, logicalDevice, swapChainImages[i], imageViewData);

        return imageViews;
    }
    
    private unsafe void DisposeVulkanObjects()
    {
        _frameBuffers.ForEach(x => x.Dispose());
        _commandBuffers.ForEach(x => x.Dispose());
        _fences.ForEach(x => x.Dispose());
        _imageViews.ForEach(x => x.Dispose());
        _syncSemaphores.ForEach(x => x.Dispose());
        _khrSwapChain.DestroySwapchain(_logicalDevice.VkLogicalDevice, _vkSwapChain, null);
        _khrSwapChain.Dispose();
    }
    
    public void Dispose()
    {
        DisposeVulkanObjects();
        GC.SuppressFinalize(this);
    }

    public void Submit(VulkanQueue queue)
    {
        var fence = _fences[_currentFrame];
        fence.Reset();
        var commandBuffer = _commandBuffers[_currentFrame];
        var syncSemaphores = _syncSemaphores[_currentFrame];

        queue.Submit(commandBuffer, syncSemaphores, fence);
    }
}

public class SwapChainSyncSemaphores: IDisposable
{
    public readonly VulkanSemaphore ImageAcquisition;
    public readonly VulkanSemaphore RenderComplete;

    public SwapChainSyncSemaphores(Vk vk, VulkanLogicalDevice logicalDevice)
    {
        ImageAcquisition = new VulkanSemaphore(vk, logicalDevice);
        RenderComplete = new VulkanSemaphore(vk, logicalDevice);
    }

    public void Dispose()
    {
        ImageAcquisition.Dispose();
        RenderComplete.Dispose();
        GC.SuppressFinalize(this);
    }
}