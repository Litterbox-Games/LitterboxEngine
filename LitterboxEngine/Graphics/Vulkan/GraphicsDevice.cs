using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.Vulkan;

public sealed class GraphicsDevice: Graphics.GraphicsDevice
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
    private readonly ForwardRenderTask _forwardRenderTask;
    private readonly PipelineCache _pipelineCache;
    
    public GraphicsDevice(Window window, GraphicsDeviceDescription description)
    {
        _vk = Vk.GetApi();
        _instance = new Instance(_vk, window.Title, true);
        _physicalDevice = PhysicalDevice.SelectPreferredPhysicalDevice(_vk, _instance);
        _logicalDevice = new LogicalDevice(_vk, _physicalDevice);
        _surface = new Surface(_vk, _instance, window);
        _graphicsQueue = new GraphicsQueue(_vk, _logicalDevice, 0);
        _presentQueue = new PresentQueue(_vk, _logicalDevice, _surface, 0);
        _swapChain = new SwapChain(_vk, _instance, _logicalDevice, _surface, window, 3, 
            false, _presentQueue, new []{_graphicsQueue});
        _commandPool = new CommandPool(_vk, _logicalDevice, _graphicsQueue.QueueFamilyIndex);
        // TODO: This could really be made as a part of the SwapChain
        // TODO: We would just need to create the commandPool before the SwapChain and pass it in
        _forwardRenderTask = new ForwardRenderTask(_vk, _swapChain, _commandPool);
        _pipelineCache = new PipelineCache(_vk, _logicalDevice);
    }
    
    public override void CreateBuffer()
    {
        throw new NotImplementedException();
    }

    public override void UpdateBuffer()
    {
        throw new NotImplementedException();
    }

    public override ShaderProgram CreateShaderProgram(params ShaderDescription[] descriptions)
    {
        return new ShaderProgram(_vk, _logicalDevice, descriptions);
    }

    public override Pipeline CreatePipeline(PipelineDescription description)
    {
        return new Pipeline(_vk, _logicalDevice, _pipelineCache, description);
    }

    public override void CreatCommandList()
    {
        throw new NotImplementedException();
    }

    public override void WaitIdle()
    {
        _logicalDevice.WaitIdle();
    }
    
    // TODO: this is for testing and should be removed later
    public override void Render()
    {
        _forwardRenderTask.WaitForFence();
        _swapChain.AcquireNextImage();
        _forwardRenderTask.Submit(_graphicsQueue);
        _swapChain.PresentImage(_presentQueue);
    }
    
    public override void Dispose()
    {
        _presentQueue.WaitIdle();
        _graphicsQueue.WaitIdle();
        _logicalDevice.WaitIdle();
        
        _pipelineCache.Dispose();
        _forwardRenderTask.Dispose();
        _commandPool.Dispose();
        _swapChain.Dispose();
        _surface.Dispose();
        _logicalDevice.Dispose();
        _instance.Dispose();
        _vk.Dispose();
        // GC.SuppressFinalize(this);
    }
}