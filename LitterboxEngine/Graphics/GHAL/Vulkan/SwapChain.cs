using MoreLinq;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class SwapChain: IDisposable
{
    public Extent2D Extent;
    private  SwapchainKHR _vkSwapChain;
    private readonly LogicalDevice _logicalDevice;
    private readonly Surface _surface;
    private readonly bool _vsyncEnabled;
    private readonly Vk _vk;
    private readonly CommandPool _commandPool;
    public readonly uint ImageCount;
    private readonly Queue[]? _concurrentQueues;
    private readonly uint[]? _concurrentFamilyIndices;
    private readonly RenderPass _renderPass;
    private readonly Window _window;
    
    private KhrSwapchain _khrSwapChain = null!;
    private ImageView[] _imageViews = null!;
    private SwapChainSyncSemaphores[] _syncSemaphores = null!;
    private FrameBuffer[] _frameBuffers = null!;
    private Fence[] _fences = null!;
    private CommandBuffer[] _commandBuffers = null!;

    public FrameBuffer CurrentFrameBuffer => _frameBuffers[_currentFrame];
    public CommandBuffer CurrentCommandBuffer => _commandBuffers[_currentFrame];

    private uint _currentFrame;

    public SwapChain(Vk vk, LogicalDevice logicalDevice, Surface surface, RenderPass renderPass, CommandPool commandPool, Window window, int requestedImages, bool vsyncEnabled,
        Queue presentQueue, Queue[]? concurrentQueues)
    {
        _vk = vk;
        _logicalDevice = logicalDevice;
        _surface = surface;
        _commandPool = commandPool;
        _vsyncEnabled = vsyncEnabled;
        _window = window;
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

    public unsafe void PresentImage(Queue queue)
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

        Extent = CalculateSwapChainExtent(_window, surfaceCapabilities);
        
        var swapChainCreateInfo = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface.VkSurface,
            MinImageCount = ImageCount,
            ImageFormat = _surface.Format.Format,
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
        _frameBuffers = _imageViews.Select(imageView => new FrameBuffer(_vk, _logicalDevice, Extent.Width, Extent.Height, imageView, _renderPass)).ToArray();
        _fences = _imageViews.Select(_ => new Fence(_vk, _logicalDevice, true)).ToArray();
        _commandBuffers = _imageViews.Select(_ => new CommandBuffer(_vk, _commandPool, true, false)).ToArray();
    }
    
    private static uint CalculateImageCount(SurfaceCapabilitiesKHR surfaceCapabilities, int requestedImages)
    {
        var maxImages = (int)surfaceCapabilities.MaxImageCount;
        var minImages = (int)surfaceCapabilities.MinImageCount;
        var result = minImages;
        if (maxImages != 0) {
            result = Math.Min(requestedImages, maxImages);
        }
        result = Math.Max(result, minImages);

        return (uint)result;
    }

    private static Extent2D CalculateSwapChainExtent(Window window, SurfaceCapabilitiesKHR surfaceCapabilities)
    {
        if (surfaceCapabilities.CurrentExtent.Width != uint.MaxValue) return surfaceCapabilities.CurrentExtent;
        
        // Surface size undefined. Set to the window size if within bounds
        var width = Math.Min(window.Width, (int)surfaceCapabilities.MaxImageExtent.Width);
        width = Math.Max(width, (int)surfaceCapabilities.MinImageExtent.Width);

        var height = Math.Min(window.Height, (int)surfaceCapabilities.MaxImageExtent.Height);
        height = Math.Max(height, (int)surfaceCapabilities.MinImageExtent.Height);
            
        
        return new Extent2D
            { Width = (uint)width, Height = (uint)height };
    }

    private static unsafe ImageView[] CreateImageViews(Vk vk, LogicalDevice logicalDevice, KhrSwapchain khrSwapchain, SwapchainKHR swapchainKhr, Format format)
    {
        uint imageCount = 0;
        var result = khrSwapchain.GetSwapchainImages(logicalDevice.VkLogicalDevice, swapchainKhr, ref imageCount, null);
        
        if (result != Result.Success)
            throw new Exception($"Failed to get surface images with error: {result.ToString()}.");

        if (imageCount == 0)
            throw new Exception($"No surface images were found");
        
        Span<Silk.NET.Vulkan.Image> swapChainImages = new Silk.NET.Vulkan.Image[imageCount];

        result = khrSwapchain.GetSwapchainImages(logicalDevice.VkLogicalDevice, swapchainKhr, &imageCount, swapChainImages);

        if (result != Result.Success)
            throw new Exception($"Failed to get surface images with error: {result.ToString()}.");

        var imageViews = new ImageView[imageCount];
        var imageViewData = new ImageView.ImageViewData(format, ImageAspectFlags.ColorBit);

        for (var i = 0; i < imageCount; i++)
            imageViews[i] = new ImageView(vk, logicalDevice, swapChainImages[i], imageViewData);

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

    public void Submit(Queue queue)
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
    public readonly Semaphore ImageAcquisition;
    public readonly Semaphore RenderComplete;

    public SwapChainSyncSemaphores(Vk vk, LogicalDevice logicalDevice)
    {
        ImageAcquisition = new Semaphore(vk, logicalDevice);
        RenderComplete = new Semaphore(vk, logicalDevice);
    }

    public void Dispose()
    {
        ImageAcquisition.Dispose();
        RenderComplete.Dispose();
        GC.SuppressFinalize(this);
    }
}