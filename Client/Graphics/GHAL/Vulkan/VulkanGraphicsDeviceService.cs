using Client.Graphics.Input;
using Client.Graphics.Input.ImGui;
using Client.Resource;
using Silk.NET.Input;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.ImGui;

namespace Client.Graphics.GHAL.Vulkan;

// TODO: where should this be disposed and should IGraphicsDeviceService inherit from IDisposable?
public sealed class VulkanGraphicsDeviceService : IGraphicsDeviceService, IDisposable
{
    public readonly Vk Vk;
    private readonly VulkanInstance _instance;
    public readonly VulkanLogicalDevice LogicalDevice;
    private readonly VulkanSurface _surface;
    private readonly VulkanRenderPass _renderPass;
    public readonly VulkanQueue GraphicsQueue;
    private readonly VulkanQueue _presentQueue;
    private readonly VulkanCommandPool _commandPool;
    public readonly VulkanSwapChain SwapChain;
    private readonly VulkanPipelineCache _pipelineCache;
    private readonly VulkanDescriptorPool _descriptorPool;
    private readonly WindowService _windowService;

    public VulkanGraphicsDeviceService(WindowService windowService)
    {
        Vk = Vk.GetApi();
        _windowService = windowService;
        _instance = new VulkanInstance(Vk, _windowService.Title, true);
        var physicalDevice = VulkanPhysicalDevice.SelectPreferredPhysicalDevice(Vk, _instance);
        LogicalDevice = new VulkanLogicalDevice(Vk, physicalDevice);
        _surface = new VulkanSurface(Vk, _instance, physicalDevice, _windowService);
        _renderPass = new VulkanRenderPass(Vk, LogicalDevice, _surface.Format.Format);
        GraphicsQueue = new GraphicsQueue(Vk, LogicalDevice, 0);
        _presentQueue = new PresentQueue(Vk, LogicalDevice, _surface, 0);
        _commandPool = new VulkanCommandPool(Vk, LogicalDevice, GraphicsQueue.QueueFamilyIndex);

        SwapChain = new VulkanSwapChain(Vk, LogicalDevice, _surface, _renderPass, _commandPool, _windowService, 3,
            false, _presentQueue, new[] {GraphicsQueue});
        _descriptorPool = new VulkanDescriptorPool(Vk, LogicalDevice);
        _pipelineCache = new VulkanPipelineCache(Vk, LogicalDevice);
        _windowService.OnResize += WindowResized;
    }

    private void WindowResized(int width, int height)
    {
        SwapChain.Recreate();
    }

    public ImGui InitImGui()
    {
        return new ImGui(_windowService, this);
    }

    public Buffer CreateBuffer(BufferDescription description)
    {
        return new VulkanBuffer(Vk, LogicalDevice, description, MemoryPropertyFlags.DeviceLocalBit, _commandPool,
            GraphicsQueue);
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
        return new VulkanShaderProgram(Vk, LogicalDevice, descriptions);
    }

    public Texture CreateTexture(uint width, uint height, Span<byte> data)
    {
        return new VulkanTexture(Vk, LogicalDevice, _commandPool, GraphicsQueue, width, height, data);
    }

    public Texture CreateTexture(uint width, uint height, RgbaByte color)
    {
        var pixelCount = (int) (width * height);
        var singlePixelColor = new[] {color.R, color.G, color.B, color.A};

        var data = Enumerable.Range(0, pixelCount)
            .Select(_ => singlePixelColor)
            .SelectMany(x => x)
            .ToArray();

        return new VulkanTexture(Vk, LogicalDevice, _commandPool, GraphicsQueue, width, height, data);
    }

    public Pipeline CreatePipeline(PipelineDescription description)
    {
        return new VulkanPipeline(Vk, LogicalDevice, _renderPass, _pipelineCache, description);
    }

    public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description)
    {
        return new VulkanDescriptorSetLayout(Vk, LogicalDevice, description);
    }

    public ResourceSet CreateResourceSet(ResourceLayout layout)
    {
        var descriptorSetLayout = (layout as VulkanDescriptorSetLayout)!;
        return new VulkanDescriptorSet(Vk, LogicalDevice, _descriptorPool, descriptorSetLayout);
    }

    public Sampler CreateSampler()
    {
        return new VulkanSampler(Vk, LogicalDevice);
    }

    public CommandList CreateCommandList()
    {
        return new VulkanCommandList(Vk, SwapChain, _renderPass);
    }

    public void SubmitCommands()
    {
        SwapChain.Submit(GraphicsQueue);
        SwapChain.PresentImage(_presentQueue);
    }

    public void SwapBuffers()
    {
        SwapChain.WaitForFence();
        SwapChain.AcquireNextImage();
    }

    public void WaitIdle()
    {
        LogicalDevice.WaitIdle();
    }

    public void Dispose()
    {
        _presentQueue.WaitIdle();
        GraphicsQueue.WaitIdle();
        LogicalDevice.WaitIdle();
     
        _windowService.OnResize -= WindowResized;
        
        _pipelineCache.Dispose();
        _descriptorPool.Dispose();
        SwapChain.Dispose();
        _commandPool.Dispose();
        _renderPass.Dispose();
        _surface.Dispose();
        LogicalDevice.Dispose();
        _instance.Dispose();
        Vk.Dispose();
    }
}