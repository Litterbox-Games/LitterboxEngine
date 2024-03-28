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
    private readonly CommandPool _commandPool;
    private readonly PipelineCache _pipelineCache;
    private readonly DescriptorPool _descriptorPool;
    private readonly Window _window;
    
    public SwapChain SwapChain;
    
    public GraphicsDevice(Window window, GraphicsDeviceDescription description)
    {
        _vk = Vk.GetApi();
        _window = window;
        _instance = new Instance(_vk, _window.Title, true);
        _physicalDevice = PhysicalDevice.SelectPreferredPhysicalDevice(_vk, _instance);
        _logicalDevice = new LogicalDevice(_vk, _physicalDevice);
        _surface = new Surface(_vk, _instance, _window);
        _graphicsQueue = new GraphicsQueue(_vk, _logicalDevice, 0);
        _presentQueue = new PresentQueue(_vk, _logicalDevice, _surface, 0);
        _commandPool = new CommandPool(_vk, _logicalDevice, _graphicsQueue.QueueFamilyIndex);
        SwapChain = new SwapChain(_vk, _instance, _logicalDevice, _surface, _commandPool, _window, 3, 
            false, _presentQueue, new []{_graphicsQueue});
        _descriptorPool = new DescriptorPool(_vk, _logicalDevice, SwapChain.ImageCount);
        _pipelineCache = new PipelineCache(_vk, _logicalDevice);
        _window.OnResize += WindowResized;
    }

    private void WindowResized(int width, int height)
    {
        _logicalDevice.WaitIdle();
        _graphicsQueue.WaitIdle();
        SwapChain.Dispose();
        SwapChain = new SwapChain(_vk, _instance, _logicalDevice, _surface, _commandPool, _window, 3, 
            false, _presentQueue, new []{_graphicsQueue});
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
        return new Pipeline(_vk, SwapChain, _pipelineCache, description);
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
        return new CommandList(_vk, this);
    }

    public override void SubmitCommands()
    {
        SwapChain.Submit(_graphicsQueue);
        if(SwapChain.PresentImage(_presentQueue)) 
            WindowResized(_window.Width, _window.Height);
    }

    public override void SwapBuffers()
    {
        SwapChain.WaitForFence();
        if(SwapChain.AcquireNextImage()) 
            WindowResized(_window.Width, _window.Height);
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
        SwapChain.Dispose();
        _commandPool.Dispose();
        _surface.Dispose();
        _logicalDevice.Dispose();
        _instance.Dispose();
        _vk.Dispose();
        // GC.SuppressFinalize(this);
    }
}