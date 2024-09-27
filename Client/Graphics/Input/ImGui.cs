using Client.Graphics.GHAL.Vulkan;
using Silk.NET.Input;
using Silk.NET.Vulkan.Extensions.ImGui;

namespace Client.Graphics.Input;

public class ImGui: IDisposable
{
    private readonly ImGuiController _imGuiController;
    private readonly IInputContext _inputContext;
    private readonly VulkanSwapChain _swapChain;
    
    
    public ImGui(GlfwWindowService windowService, VulkanGraphicsDeviceService graphicsDeviceService)
    {
        _swapChain = graphicsDeviceService.SwapChain;
        
        _imGuiController = new ImGuiController(
            graphicsDeviceService.Vk,
            windowService.Window,
            _inputContext = windowService.Window.CreateInput(),
            graphicsDeviceService.LogicalDevice.PhysicalDevice.VkPhysicalDevice,
            graphicsDeviceService.GraphicsQueue.QueueFamilyIndex,
            _swapChain.ImageCount,
            _swapChain.Format,
            null
        );
    }

    public void Update(float deltaTime)
    {
        _imGuiController.Update(deltaTime);
        
        // This is where you'll tell ImGui what to draw.
        // For now, we'll just show their built-in demo window.
        ImGuiNET.ImGui.ShowDemoWindow();
    }

    public void Draw()
    {
        _imGuiController.Render(_swapChain.CurrentCommandBuffer.VkCommandBuffer,
            _swapChain.CurrentFrameBuffer.VkFrameBuffer, _swapChain.Extent);
    }

    public void Dispose()
    {
        _imGuiController.Dispose();
        _inputContext.Dispose();
        GC.SuppressFinalize(this);
    }
}