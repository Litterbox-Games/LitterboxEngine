using Client.Graphics.GHAL.Vulkan;
using Silk.NET.Vulkan.Extensions.ImGui;

namespace Client.Graphics.Input.ImGui;

public class ImGuiRenderer: IDisposable
{
    private readonly ImGuiController _imGuiController;
    private readonly VulkanSwapChain _swapChain;
    
    public ImGuiRenderer(Window window, VulkanGraphicsDevice graphicsDevice)
    {
        _swapChain = graphicsDevice.SwapChain;
        
        _imGuiController = new ImGuiController(
            graphicsDevice.Vk,
            window.InternalWindow,
            window.Input,
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