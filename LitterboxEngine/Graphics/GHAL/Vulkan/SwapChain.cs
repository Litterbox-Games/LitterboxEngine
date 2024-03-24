using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class SwapChain: IDisposable
{
    public readonly Extent2D Extent;
    private readonly KhrSwapchain _khrSwapChain;
    public readonly SwapchainKHR VkSwapChain;
    private readonly ImageView[] _imageViews;
    public readonly LogicalDevice LogicalDevice;
    
    private readonly SwapChainSyncSemaphores[] _syncSemaphores;
    public readonly SwapChainRenderPass RenderPass;
    private readonly FrameBuffer[] _frameBuffers;
    private readonly Fence[] _fences;
    private readonly CommandBuffer[] _commandBuffers;

    public FrameBuffer CurrentFrameBuffer => _frameBuffers[CurrentFrame];
    public CommandBuffer CurrentCommandBuffer => _commandBuffers[CurrentFrame];
    public uint ImageCount => (uint)_imageViews.Length;
    
    public uint CurrentFrame { get; private set; }

    public unsafe SwapChain(Vk vk, Instance instance, LogicalDevice logicalDevice, Surface surface, CommandPool commandPool, Window window, int requestedImages, bool vsync,
        Queue presentQueue, Queue[]? concurrentQueues)
    {
        LogicalDevice = logicalDevice;
        
        var physicalDevice = LogicalDevice.PhysicalDevice;
        surface.KhrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice.VkPhysicalDevice, surface.VkSurface,
            out var surfaceCapabilities);

        var imageCount = CalcImageCount(surfaceCapabilities, requestedImages);

        var surfaceFormat = CalcSurfaceFormat(physicalDevice, surface);

        Extent = CalcSwapChainExtent(window, surfaceCapabilities);
        
        var swapChainCreateInfo = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = surface.VkSurface,
            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = Extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = surfaceCapabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            Clipped = true,
            PresentMode = vsync ? PresentModeKHR.FifoKhr : PresentModeKHR.ImmediateKhr
        };

        if (!vk.TryGetDeviceExtension(instance.VkInstance, LogicalDevice.VkLogicalDevice, out _khrSwapChain))
            throw new Exception("VK_KHR_swapchain extension was not found or could not be loaded");

        var concurrentFamilyIndices = concurrentQueues?
            .Select(queue => queue.QueueFamilyIndex)
            .Where(queueFamilyIndex => queueFamilyIndex != presentQueue.QueueFamilyIndex)
            .ToArray();

        if (concurrentFamilyIndices != null && concurrentFamilyIndices.Length > 0)
        {
            fixed (uint* concurrentFamilyIndicesPtr = concurrentFamilyIndices)
            {
                swapChainCreateInfo.ImageSharingMode = SharingMode.Concurrent;
                swapChainCreateInfo.QueueFamilyIndexCount = (uint)concurrentFamilyIndices.Length;
                swapChainCreateInfo.PQueueFamilyIndices = concurrentFamilyIndicesPtr;    
            }
        }
        
        var result = _khrSwapChain.CreateSwapchain(LogicalDevice.VkLogicalDevice, swapChainCreateInfo, null, out VkSwapChain);
        if (result != Result.Success)
            throw new Exception($"Failed to create swap chain with error: {result.ToString()}.");
        
        _imageViews = CreateImageViews(vk, LogicalDevice, _khrSwapChain, VkSwapChain, surfaceFormat.Format);
        _syncSemaphores = _imageViews.Select(_ => new SwapChainSyncSemaphores(vk, LogicalDevice)).ToArray();
        
        RenderPass = new SwapChainRenderPass(vk, LogicalDevice, surfaceFormat.Format);

        // Create a frame buffer for each swap chain image
        _frameBuffers = _imageViews
            .Select(imageView => new FrameBuffer(vk, logicalDevice, Extent.Width, Extent.Height, imageView, RenderPass))
            .ToArray();

        // Create a fence for each swap chain image
        _fences = _imageViews
            .Select(_ => new Fence(vk, logicalDevice, true))
            .ToArray();
        
        // Create a command buffer for each swap chain image
        _commandBuffers = _imageViews
            .Select(_ => new CommandBuffer(vk, commandPool, true, false))
            .ToArray();
    }

    // Returns true if we need to resize/recreate the swap chain otherwise false
    public bool AcquireNextImage()
    {
        uint imageIndex = 0;
        var result = _khrSwapChain.AcquireNextImage(LogicalDevice.VkLogicalDevice, VkSwapChain, ulong.MaxValue, 
            _syncSemaphores[CurrentFrame].ImageAcquisition.VkSemaphore, default, ref imageIndex);

        CurrentFrame = imageIndex;
        
        if (result == Result.ErrorOutOfDateKhr) return true;
        
        if (result != Result.Success && result != Result.SuboptimalKhr)
            throw new Exception($"Failed to acquire swap chain image with error: ${result.ToString()}");

        return false;
    }

    // Returns true if we need to resize/recreate the swap chain otherwise false
    public unsafe bool PresentImage(Queue queue)
    {
        var imageIndex = CurrentFrame;
        var signalSemaphores = stackalloc[] { _syncSemaphores[CurrentFrame].RenderComplete.VkSemaphore };
        var swapChains = stackalloc[] { VkSwapChain };
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
        
        CurrentFrame = (CurrentFrame + 1) % (uint)_imageViews.Length;
        
        if (result == Result.ErrorOutOfDateKhr) return true;
        
        if (result != Result.Success && result != Result.SuboptimalKhr)
            throw new Exception($"Failed to present KHR with error: ${result.ToString()}");
        
        return false;
    }
    
    public void WaitForFence()
    {
        _fences[CurrentFrame].Wait();
    }

    private static uint CalcImageCount(SurfaceCapabilitiesKHR surfaceCapabilities, int requestedImages)
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

    private static unsafe SurfaceFormatKHR CalcSurfaceFormat(PhysicalDevice physicalDevice, Surface surface)
    {
        uint formatCount = 0;
        var result = surface.KhrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice.VkPhysicalDevice, 
            surface.VkSurface, ref formatCount, null);
        
        if (result != Result.Success)
            throw new Exception($"Failed to get physical device formats with error: {result.ToString()}.");
        
        if (formatCount == 0)
            throw new Exception("No available formats for selected physical device");

        var formats = new SurfaceFormatKHR[formatCount];
        fixed (SurfaceFormatKHR* formatsPtr = formats)
        {
            result = surface.KhrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice.VkPhysicalDevice, 
                surface.VkSurface, ref formatCount, formatsPtr);
        
            if (result != Result.Success)
                throw new Exception(
                    $"Failed to get physical device surface formats with error: {result.ToString()}.");
        }

        foreach (var format in formats)
        {
            if (format.Format != Format.B8G8R8A8Srgb || 
                format.ColorSpace != ColorSpaceKHR.SpaceSrgbNonlinearKhr) continue;
            return format;
        }

        return formats[0];
    }

    private static Extent2D CalcSwapChainExtent(Window window, SurfaceCapabilitiesKHR surfaceCapabilities)
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
        
        Span<Image> swapChainImages = new Image[imageCount];

        result = khrSwapchain.GetSwapchainImages(logicalDevice.VkLogicalDevice, swapchainKhr, &imageCount, swapChainImages);

        if (result != Result.Success)
            throw new Exception($"Failed to get surface images with error: {result.ToString()}.");

        var imageViews = new ImageView[imageCount];
        var imageViewData = new ImageView.ImageViewData(format, ImageAspectFlags.ColorBit);

        for (var i = 0; i < imageCount; i++)
            imageViews[i] = new ImageView(vk, logicalDevice, swapChainImages[i], imageViewData);

        return imageViews;
    }

    public unsafe void Dispose()
    {
        foreach (var frameBuffer in _frameBuffers) 
            frameBuffer.Dispose();
        
        RenderPass.Dispose();
        
        foreach (var commandBuffer in _commandBuffers) 
            commandBuffer.Dispose();
        
        foreach (var fence in _fences) 
            fence.Dispose();
        
        foreach (var imageView in _imageViews) 
            imageView.Dispose();

        foreach (var syncSemaphore in _syncSemaphores) 
            syncSemaphore.Dispose();
        
        _khrSwapChain.DestroySwapchain(LogicalDevice.VkLogicalDevice, VkSwapChain, null);
        _khrSwapChain.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Submit(Queue queue)
    {
        var fence = _fences[CurrentFrame];
        fence.Reset();
        var commandBuffer = _commandBuffers[CurrentFrame];
        var syncSemaphores = _syncSemaphores[CurrentFrame];

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
    }
}