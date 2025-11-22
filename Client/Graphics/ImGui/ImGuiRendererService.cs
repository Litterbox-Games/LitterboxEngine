using Client.Graphics.Backend.Vulkan;
using Common.Core;
using Silk.NET.Vulkan.Extensions.ImGui;

namespace Client.Graphics.ImGui;

[Engine]
public class ImGuiRendererService: IService
{
    private readonly ImGuiController _imGuiController;
    private readonly VulkanSwapChain _swapChain;
    
    public ImGuiRendererService(WindowService windowService, VulkanGraphicsDeviceService graphicsDevice)
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
    
    [Priority(EPriority.Low)]
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