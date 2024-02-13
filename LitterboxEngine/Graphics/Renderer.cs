using Silk.NET.Vulkan;
using Instance = LitterboxEngine.Graphics.Vulkan.Instance;

namespace LitterboxEngine.Graphics;

public class Renderer: IDisposable
{
    private readonly Vk _vk;
    private readonly Instance _instance;
    
    public Renderer(Window window)
    {
        _vk = Vk.GetApi();
        _instance = new Instance(_vk, window.Title, true);
    }

    public void Dispose()
    {
        _instance.Dispose();
        _vk.Dispose();
        GC.SuppressFinalize(this);
    }
}