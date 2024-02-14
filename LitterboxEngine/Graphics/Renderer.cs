using Silk.NET.Vulkan;
using Instance = LitterboxEngine.Graphics.Vulkan.Instance;
using PhysicalDevice = LitterboxEngine.Graphics.Vulkan.PhysicalDevice;

namespace LitterboxEngine.Graphics;

public class Renderer: IDisposable
{
    private readonly Vk _vk;
    private readonly Instance _instance;
    private readonly PhysicalDevice _physicalDevice;
    
    public Renderer(Window window)
    {
        _vk = Vk.GetApi();
        _instance = new Instance(_vk, window.Title, true);
        _physicalDevice = PhysicalDevice.SelectPreferredPhysicalDevice(_vk, _instance);
    }

    public void Dispose()
    {
        _instance.Dispose();
        _vk.Dispose();
        GC.SuppressFinalize(this);
    }
}