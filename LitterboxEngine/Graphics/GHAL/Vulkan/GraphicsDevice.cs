using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public sealed class GraphicsDevice: GHAL.GraphicsDevice
{
    private readonly Vk _vk;
    private readonly Instance _instance;
    private readonly PhysicalDevice _physicalDevice;
    private readonly LogicalDevice _logicalDevice;
    private readonly Surface _surface;
    private readonly Queue _graphicsQueue;
    private readonly Queue _presentQueue;
    private readonly SwapChain _swapChain;
    private readonly CommandPool _commandPool;
    private readonly PipelineCache _pipelineCache;
    private readonly DescriptorPool _descriptorPool;
    
    public GraphicsDevice(Window window, GraphicsDeviceDescription description)
    {
        _vk = Vk.GetApi();
        _instance = new Instance(_vk, window.Title, true);
        _physicalDevice = PhysicalDevice.SelectPreferredPhysicalDevice(_vk, _instance);
        _logicalDevice = new LogicalDevice(_vk, _physicalDevice);
        _surface = new Surface(_vk, _instance, window);
        _graphicsQueue = new GraphicsQueue(_vk, _logicalDevice, 0);
        _presentQueue = new PresentQueue(_vk, _logicalDevice, _surface, 0);
        _commandPool = new CommandPool(_vk, _logicalDevice, _graphicsQueue.QueueFamilyIndex);
        _swapChain = new SwapChain(_vk, _instance, _logicalDevice, _surface, _commandPool, window, 3, 
            false, _presentQueue, new []{_graphicsQueue});
        _descriptorPool = new DescriptorPool(_vk, _logicalDevice, _swapChain.ImageCount);
        _pipelineCache = new PipelineCache(_vk, _logicalDevice);
    }

    public override GHAL.Buffer CreateBuffer(BufferDescription description)
    {
        return new Buffer(_vk, _logicalDevice, description, MemoryPropertyFlags.DeviceLocalBit, _commandPool, _graphicsQueue);
    }

    public override void UpdateBuffer(GHAL.Buffer buffer, uint offset, uint[] data)
    {
        buffer.Update(offset, data);
    }

    public override ShaderProgram CreateShaderProgram(params ShaderDescription[] descriptions)
    {
        return new ShaderProgram(_vk, _logicalDevice, descriptions);
    }

    public override Resources.Texture CreateTexture(uint width, uint height, Span<byte> data)
    {
        return new Texture(_vk, _logicalDevice, _commandPool, _graphicsQueue, width, height, data);
    }

    public override Resources.Texture CreateTexture(uint width, uint height, RgbaByte color)
    {
        return new Texture(_vk, _logicalDevice, _commandPool, _graphicsQueue, width, height, color);
    }

    public override Pipeline CreatePipeline(PipelineDescription description)
    {
        return new Pipeline(_vk, _swapChain, _pipelineCache, description);
    }

    public override ResourceLayout CreateResourceLayout(ResourceLayoutDescription description)
    {
        return new DescriptorSetLayout(_vk, _logicalDevice, description);
    }

    public override ResourceSet CreateResourceSet(ResourceLayout layout)
    {
        var descriptorSetLayout = (layout as DescriptorSetLayout)!;
        return new DescriptorSet(_vk, _logicalDevice, _descriptorPool, descriptorSetLayout);
    }

    public override GHAL.Sampler CreateSampler()
    {
        return new Sampler(_vk, _logicalDevice);
    }

    public override CommandList CreateCommandList()
    {
        return new CommandList(_vk, _swapChain);
    }

    public override void SubmitCommands()
    {
        _swapChain.Submit(_graphicsQueue);
        _swapChain.PresentImage(_presentQueue);
    }

    public override void SwapBuffers()
    {
        _swapChain.WaitForFence();
        _swapChain.AcquireNextImage();
    }

    public override void WaitIdle()
    {
        _logicalDevice.WaitIdle();
    }

    public override void Dispose()
    {
        _presentQueue.WaitIdle();
        _graphicsQueue.WaitIdle();
        _logicalDevice.WaitIdle();
        
        _pipelineCache.Dispose();
        _swapChain.Dispose();
        _commandPool.Dispose();
        _surface.Dispose();
        _logicalDevice.Dispose();
        _instance.Dispose();
        _vk.Dispose();
        // GC.SuppressFinalize(this);
    }
}