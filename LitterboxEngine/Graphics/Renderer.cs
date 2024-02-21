using LitterboxEngine.Graphics.Vulkan;
using Silk.NET.Vulkan;
using Instance = LitterboxEngine.Graphics.Vulkan.Instance;
using PhysicalDevice = LitterboxEngine.Graphics.Vulkan.PhysicalDevice;
using Queue = LitterboxEngine.Graphics.Vulkan.Queue;

namespace LitterboxEngine.Graphics;

public class Renderer: IDisposable
{
    private readonly Vk _vk;
    private readonly Instance _instance;
    private readonly PhysicalDevice _physicalDevice;
    private readonly LogicalDevice _logicalDevice;
    private readonly Surface _surface;
    private readonly Queue _graphicsQueue;
    private readonly SwapChain _swapChain;
    
    public Renderer(Window window)
    {
        _vk = Vk.GetApi();
        _instance = new Instance(_vk, window.Title, true);
        _physicalDevice = PhysicalDevice.SelectPreferredPhysicalDevice(_vk, _instance);
        _logicalDevice = new LogicalDevice(_vk, _physicalDevice);
        _surface = new Surface(_vk, _instance, window);
        _graphicsQueue = new GraphicsQueue(_vk, _logicalDevice, 0);
        _swapChain = new SwapChain(_vk, _instance, _logicalDevice, _surface, window, 3, false);
    }

    public void DeviceWaitIdle()
    {
        _logicalDevice.WaitIdle();
    }
    
    public void Dispose()
    {
        _swapChain.Dispose();
        _surface.Dispose();
        _logicalDevice.Dispose();
        _instance.Dispose();
        _vk.Dispose();
        GC.SuppressFinalize(this);
    }
}