using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.Vulkan;

// TODO: This class should become the CommandList
public class ForwardRenderTask: IDisposable
{
    private readonly SwapChainRenderPass _renderPass;
    private readonly FrameBuffer[] _frameBuffers;
    private readonly Fence[] _fences;
    private readonly CommandBuffer[] _commandBuffers;
    private readonly Vk _vk;
    private readonly SwapChain _swapChain;
    

    public ForwardRenderTask(Vk vk, SwapChain swapChain, CommandPool commandPool)
    {
        _vk = vk;
        _swapChain = swapChain;
        
        var extent = swapChain.Extent;
        var logicalDevice = swapChain.LogicalDevice;

        _renderPass = new SwapChainRenderPass(_vk, swapChain);

        // Create a frame buffer for each swap chain image
        _frameBuffers = swapChain.ImageViews
            .Select(imageView => new FrameBuffer(_vk, logicalDevice, extent.Width, extent.Height, imageView, _renderPass))
            .ToArray();

        // Create a fence for each swap chain image
        _fences = swapChain.ImageViews
            .Select(_ => new Fence(_vk, logicalDevice, true))
            .ToArray();
        
        // Create a command buffer for each swap chain image
        _commandBuffers = _frameBuffers
            .Select(frameBuffer => {
                var commandBuffer = new CommandBuffer(_vk, commandPool, true, false);
                RecordCommandBuffer(commandBuffer, frameBuffer, extent);
                return commandBuffer;
            })
            .ToArray();
    }


    private unsafe void RecordCommandBuffer(CommandBuffer commandBuffer, FrameBuffer framebuffer, Extent2D extent)
    {
        ClearValue clearColor = new()
        {
            Color = new ClearColorValue
                { Float32_0 = 1, Float32_1 = 0, Float32_2 = 0, Float32_3 = 1 },
        };
        
        RenderPassBeginInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _renderPass.VkRenderPass,
            Framebuffer = framebuffer.VkFrameBuffer,
            RenderArea =
            {
                Offset = { X = 0, Y = 0 },
                Extent = extent
            },
            ClearValueCount = 1,
            PClearValues = &clearColor
        };
        
        commandBuffer.BeginRecording();
        _vk.CmdBeginRenderPass(commandBuffer.VkCommandBuffer, &renderPassInfo, SubpassContents.Inline);
        _vk.CmdEndRenderPass(commandBuffer.VkCommandBuffer);
        commandBuffer.EndRecording();
    }


    public void Submit(Queue queue)
    {
        var fence = _fences[_swapChain.CurrentFrame];
        fence.Reset();
        var commandBuffer = _commandBuffers[_swapChain.CurrentFrame];
        var syncSemaphores = _swapChain.SyncSemaphores[_swapChain.CurrentFrame];

        queue.Submit(commandBuffer, syncSemaphores, fence);
    }

    public void WaitForFence()
    {
        _fences[_swapChain.CurrentFrame].Wait();
    }

    public void Dispose()
    {
        foreach (var frameBuffer in _frameBuffers) frameBuffer.Dispose();
        _renderPass.Dispose();
        foreach (var commandBuffer in _commandBuffers) commandBuffer.Dispose();
        foreach (var fence in _fences) fence.Dispose();
        GC.SuppressFinalize(this);
    }
}