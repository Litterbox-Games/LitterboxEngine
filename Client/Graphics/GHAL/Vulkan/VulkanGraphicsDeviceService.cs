using Client.Graphics.Input;
using Client.Resource;
using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

// TODO: where should this be disposed and should IGraphicsDeviceService inherit from IDisposable?
public sealed class VulkanGraphicsDeviceService: IGraphicsDeviceService, IDisposable
{
    private readonly Vk _vk;
    private readonly VulkanInstance _instance;
    private readonly VulkanLogicalDevice _logicalDevice;
    private readonly VulkanSurface _surface;
    private readonly VulkanRenderPass _renderPass;
    private readonly VulkanQueue _graphicsQueue;
    private readonly VulkanQueue _presentQueue;
    private readonly VulkanCommandPool _commandPool;
    private readonly VulkanSwapChain _swapChain;
    private readonly VulkanPipelineCache _pipelineCache;
    private readonly VulkanDescriptorPool _descriptorPool;
    private readonly GlfwWindowService _windowService;
    
    public VulkanGraphicsDeviceService(GlfwWindowService windowService)
    {
        _vk = Vk.GetApi();
        _windowService = windowService;
        _instance = new VulkanInstance(_vk, _windowService.Title, true);
        var physicalDevice = VulkanPhysicalDevice.SelectPreferredPhysicalDevice(_vk, _instance);
        _logicalDevice = new VulkanLogicalDevice(_vk, physicalDevice);
        _surface = new VulkanSurface(_vk, _instance, physicalDevice, _windowService);
        _renderPass =  new VulkanRenderPass(_vk, _logicalDevice, _surface.Format.Format);
        _graphicsQueue = new GraphicsQueue(_vk, _logicalDevice, 0);
        _presentQueue = new PresentQueue(_vk, _logicalDevice, _surface, 0);
        _commandPool = new VulkanCommandPool(_vk, _logicalDevice, _graphicsQueue.QueueFamilyIndex);
        _swapChain = new VulkanSwapChain(_vk, _logicalDevice, _surface, _renderPass, _commandPool, _windowService, 3, 
            true, _presentQueue, new []{_graphicsQueue});
        _descriptorPool = new VulkanDescriptorPool(_vk, _logicalDevice);
        _pipelineCache = new VulkanPipelineCache(_vk, _logicalDevice);
        _windowService.OnResize += WindowResized;
    }

    private void WindowResized(int width, int height)
    {
        _swapChain.Recreate();
    }
    
    public Buffer CreateBuffer(BufferDescription description)
    {
        return new VulkanBuffer(_vk, _logicalDevice, description, MemoryPropertyFlags.DeviceLocalBit, _commandPool, _graphicsQueue);
    }

    public void UpdateBuffer(Buffer buffer, uint offset, uint[] data)
    {
        buffer.Update(offset, data);
        
        /*
        var vkBuffer = (buffer as Buffer)!;
        void* dataPtr;
        _vk.MapMemory(_logicalDevice.VkLogicalDevice, vkBuffer.StagingBuffer.VkBufferMemory, 0, sizeof(uint) * (ulong)data.Length, 0, &dataPtr);
        data.AsSpan().CopyTo(new Span<uint>(dataPtr, data.Length));
        _vk.UnmapMemory(_logicalDevice.VkLogicalDevice, vkBuffer.StagingBuffer.VkBufferMemory);
        
        using var commandBuffer = new CommandBuffer(_vk, _commandPool, true, true);
        commandBuffer.BeginRecording();
        BufferCopy copyRegion = new() { DstOffset = offset, Size = sizeof(uint) * (ulong)data.Length };
        _vk.CmdCopyBuffer(commandBuffer.VkCommandBuffer, vkBuffer.StagingBuffer.VkBuffer, vkBuffer.VkBuffer, 1, copyRegion);
        commandBuffer.EndRecording();
        
        using var fence = new Fence(_vk, _logicalDevice, true);
        fence.Reset();
        _graphicsQueue.Submit(commandBuffer, null, fence);
        fence.Wait();
        */
    }

    public ShaderProgram CreateShaderProgram(params ShaderDescription[] descriptions)
    {
        return new VulkanShaderProgram(_vk, _logicalDevice, descriptions);
    }

    public Texture CreateTexture(uint width, uint height, Span<byte> data)
    {
        return new VulkanTexture(_vk, _logicalDevice, _commandPool, _graphicsQueue, width, height, data);
    }

    public Texture CreateTexture(uint width, uint height, RgbaByte color)
    {
        var pixelCount = (int)(width * height);
        var singlePixelColor = new[] { color.R, color.G, color.B, color.A };
        var data = Enumerable.Range(0, pixelCount)
            .Select(_ => singlePixelColor)
            .SelectMany(x => x)
            .ToArray();

        return new VulkanTexture(_vk, _logicalDevice, _commandPool, _graphicsQueue, width, height, data);
    }

    public Pipeline CreatePipeline(PipelineDescription description)
    {
        return new VulkanPipeline(_vk, _logicalDevice, _renderPass, _pipelineCache, description);
    }

    public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description)
    {
        return new VulkanDescriptorSetLayout(_vk, _logicalDevice, description);
    }

    public ResourceSet CreateResourceSet(ResourceLayout layout)
    {
        var descriptorSetLayout = (layout as VulkanDescriptorSetLayout)!;
        return new VulkanDescriptorSet(_vk, _logicalDevice, _descriptorPool, descriptorSetLayout);
    }

    public Sampler CreateSampler()
    {
        return new VulkanSampler(_vk, _logicalDevice);
    }

    public CommandList CreateCommandList()
    {
        return new VulkanCommandList(_vk, _swapChain, _renderPass);
    }

    public void SubmitCommands()
    {
        _swapChain.Submit(_graphicsQueue);
        _swapChain.PresentImage(_presentQueue);
    }

    public void SwapBuffers()
    {
        _swapChain.WaitForFence();
        _swapChain.AcquireNextImage();
    }

    public void WaitIdle()
    {
        _logicalDevice.WaitIdle();
    }

    public void Dispose()
    {
        _presentQueue.WaitIdle();
        _graphicsQueue.WaitIdle();
        _logicalDevice.WaitIdle();
     
        _windowService.OnResize -= WindowResized;
        
        _pipelineCache.Dispose();
        _descriptorPool.Dispose();
        _swapChain.Dispose();
        _commandPool.Dispose();
        _renderPass.Dispose();
        _surface.Dispose();
        _logicalDevice.Dispose();
        _instance.Dispose();
        _vk.Dispose();
    }
}