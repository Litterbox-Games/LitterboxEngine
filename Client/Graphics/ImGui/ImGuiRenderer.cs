using Client.Graphics.GHAL.Vulkan;
using Silk.NET.Vulkan.Extensions.ImGui;

namespace Client.Graphics.ImGui;

public class ImGuiRenderer: IDisposable
{
    private readonly ImGuiController _imGuiController;
    private readonly VulkanSwapChain _swapChain;
    
    public ImGuiRenderer(WindowService windowService, VulkanGraphicsDeviceService graphicsDevice)
    {
        _swapChain = graphicsDevice.SwapChain;
        
        _imGuiController = new ImGuiController(
            graphicsDevice.Vk,
            windowService.InternalWindow,
            windowService.Input,
            graphicsDevice.LogicalDevice.PhysicalDevice.VkPhysicalDevice,
            graphicsDevice.GraphicsQueue.QueueFamilyIndex,
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