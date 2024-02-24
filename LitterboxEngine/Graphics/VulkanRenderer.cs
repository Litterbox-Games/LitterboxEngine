using LitterboxEngine.Graphics.Vulkan;
using Silk.NET.Vulkan;
using Instance = LitterboxEngine.Graphics.Vulkan.Instance;
using PhysicalDevice = LitterboxEngine.Graphics.Vulkan.PhysicalDevice;
using Queue = LitterboxEngine.Graphics.Vulkan.Queue;
using CommandPool = LitterboxEngine.Graphics.Vulkan.CommandPool;

namespace LitterboxEngine.Graphics;

public class VulkanRenderer: IDisposable
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
    
    public VulkanRenderer(Window window)
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
        
    }

    public void Render()
    {
        _forwardRenderTask.WaitForFence();
        _swapChain.AcquireNextImage();
        _forwardRenderTask.Submit(_graphicsQueue);
        _swapChain.PresentImage(_presentQueue);
    }
    
    public void DeviceWaitIdle()
    {
        _logicalDevice.WaitIdle();
    }
    
    public void Dispose()
    {
        _presentQueue.WaitIdle();
        _graphicsQueue.WaitIdle();
        _logicalDevice.WaitIdle();
        
        _forwardRenderTask.Dispose();
        _commandPool.Dispose();
        _swapChain.Dispose();
        _surface.Dispose();
        _logicalDevice.Dispose();
        _instance.Dispose();
        _vk.Dispose();
        GC.SuppressFinalize(this);
    }
}