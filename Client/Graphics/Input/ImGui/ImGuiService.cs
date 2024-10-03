using Client.Graphics.GHAL.Vulkan;
using Common.DI;
using Silk.NET.Vulkan.Extensions.ImGui;

namespace Client.Graphics.Input.ImGui;

public class ImGuiService: IService, IDisposable
{
    private readonly ImGuiController _imGuiController;
    private readonly VulkanSwapChain _swapChain;
    
    
    public ImGuiService(WindowService windowService, VulkanGraphicsDeviceService graphicsDeviceService)
    {
        _swapChain = graphicsDeviceService.SwapChain;
        
        _imGuiController = new ImGuiController(
            graphicsDeviceService.Vk,
            windowService.Window,
            windowService.Input,
            graphicsDeviceService.LogicalDevice.PhysicalDevice.VkPhysicalDevice,
            graphicsDeviceService.GraphicsQueue.QueueFamilyIndex,
            _swapChain.ImageCount,
            _swapChain.Format,
            null,
            null
        );
    }

    public void Update(float deltaTime)
    {
        _imGuiController.Update(deltaTime);
    }

    public void Draw()
    {
        _imGuiController.Render(_swapChain.CurrentCommandBuffer.VkCommandBuffer,
            _swapChain.CurrentFrameBuffer.VkFrameBuffer, _swapChain.Extent);
    }

    public void Dispose()
    {
        _imGuiController.Dispose();
        GC.SuppressFinalize(this);
    }
}