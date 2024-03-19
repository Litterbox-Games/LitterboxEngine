using System.Drawing;
using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class CommandList: GHAL.CommandList
{

    private readonly Vk _vk;
    private readonly SwapChain _swapChain;
    
    public CommandList(Vk vk, SwapChain swapChain)
    {
        _vk = vk;
        _swapChain = swapChain;
    }

    public override unsafe void Begin(Color clearColor)
    {
        ClearValue clearValue = new()
        {
            Color = new ClearColorValue
                { Float32_0 = clearColor.R / 255f, Float32_1 = clearColor.G / 255f, Float32_2 = clearColor.B / 255f, Float32_3 = 1 },
        };
        
        RenderPassBeginInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _swapChain.RenderPass.VkRenderPass,
            Framebuffer = _swapChain.CurrentFrameBuffer.VkFrameBuffer,
            RenderArea =
            {
                Offset = { X = 0, Y = 0 },
                Extent = _swapChain.Extent
            },
            ClearValueCount = 1,
            PClearValues = &clearValue
        };
        
        _swapChain.CurrentCommandBuffer.BeginRecording();
        _vk.CmdBeginRenderPass(_swapChain.CurrentCommandBuffer.VkCommandBuffer, &renderPassInfo, SubpassContents.Inline);
        
        // bind pipeline
        
        // bind vertex buffer
        // bind index buffer
        // draw indexed
    }

    public override void End()
    {
        _vk.CmdEndRenderPass(_swapChain.CurrentCommandBuffer.VkCommandBuffer);
        _swapChain.CurrentCommandBuffer.EndRecording();
    }

    public override void SetPipeline(GHAL.Pipeline pipeline)
    {
        if (pipeline is not Pipeline vulkanPipeline)
            throw new Exception("Cannot use non-vulkan pipeline with a vulkan command list");
        
        _vk.CmdBindPipeline(_swapChain.CurrentCommandBuffer.VkCommandBuffer, PipelineBindPoint.Graphics, vulkanPipeline.VkPipeline);
    }

    public override void SetIndexBuffer()
    {
        throw new NotImplementedException();
    }

    public override void UpdateBuffer()
    {
        throw new NotImplementedException();
    }

    public override void SetVertexBuffer()
    {
        throw new NotImplementedException();
    }

    public override void DrawIndexed()
    {
        throw new NotImplementedException();
    }
}