using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class CommandList: GHAL.CommandList
{

    private readonly Vk _vk;
    private SwapChain SwapChain => _graphicsDevice.SwapChain;
    private readonly GraphicsDevice _graphicsDevice;

    private Pipeline? _pipeline;

    public CommandList(Vk vk, GraphicsDevice graphicsDevice)
    {
        _vk = vk;
        _graphicsDevice = graphicsDevice;
        //_swapChain = swapChain;
    }

    public override unsafe void Begin(RgbaFloat clearColor)
    {
        ClearValue clearValue = new()
        {
            Color = new ClearColorValue
                { Float32_0 = clearColor.R, Float32_1 = clearColor.G, Float32_2 = clearColor.B, Float32_3 = 1 },
        };
        
        RenderPassBeginInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = SwapChain.RenderPass.VkRenderPass,
            Framebuffer = SwapChain.CurrentFrameBuffer.VkFrameBuffer,
            RenderArea =
            {
                Offset = { X = 0, Y = 0 },
                Extent = SwapChain.Extent
            },
            ClearValueCount = 1,
            PClearValues = &clearValue
        };
        
        SwapChain.CurrentCommandBuffer.BeginRecording();
        _vk.CmdBeginRenderPass(SwapChain.CurrentCommandBuffer.VkCommandBuffer, &renderPassInfo, SubpassContents.Inline);
    }

    public override void End()
    {
        _vk.CmdEndRenderPass(SwapChain.CurrentCommandBuffer.VkCommandBuffer);
        SwapChain.CurrentCommandBuffer.EndRecording();
    }

    public override void SetPipeline(GHAL.Pipeline pipeline)
    {
        if (pipeline is not Pipeline vulkanPipeline)
            throw new Exception("Cannot use non-vulkan pipeline with a vulkan command list");

        _pipeline = vulkanPipeline;

        _vk.CmdBindPipeline(SwapChain.CurrentCommandBuffer.VkCommandBuffer, PipelineBindPoint.Graphics, vulkanPipeline.VkPipeline);
        
        Viewport viewport = new()
        {
            X = 0,
            Y = 0,
            Width = SwapChain.Extent.Width,
            Height = SwapChain.Extent.Height,
            MinDepth = 0,
            MaxDepth = 1
        };
            
        Rect2D scissor = new()
        {
            Offset = { X = 0, Y = 0 },
            Extent = SwapChain.Extent
        };

        _vk.CmdSetViewport(SwapChain.CurrentCommandBuffer.VkCommandBuffer, 0, 1, in viewport);
        _vk.CmdSetScissor(SwapChain.CurrentCommandBuffer.VkCommandBuffer, 0, 1, in scissor);
    }

    public override void SetIndexBuffer(GHAL.Buffer buffer, IndexFormat format)
    {
        var indexBuffer = (buffer as Buffer)!.VkBuffer;
        _vk.CmdBindIndexBuffer(SwapChain.CurrentCommandBuffer.VkCommandBuffer, indexBuffer, 0, IndexTypeFromIndexFormat(format));
    }

    public override unsafe void SetVertexBuffer(ulong offset, GHAL.Buffer buffer)
    {
        var vertexBuffers = stackalloc[] { (buffer as Buffer)!.VkBuffer };
        var offsets = stackalloc[] {offset};
        _vk.CmdBindVertexBuffers(SwapChain.CurrentCommandBuffer.VkCommandBuffer, 0, 1, vertexBuffers, offsets);        
    }

    public override void UpdateBuffer<T>(GHAL.Buffer buffer, ulong offset, T data)
    {
        buffer.Update(offset, data);
    }

    public override void UpdateBuffer<T>(GHAL.Buffer buffer, ulong offset, T[] data)
    {
        buffer.Update(offset, data);
    }

    private static IndexType IndexTypeFromIndexFormat(IndexFormat format)
    {
        return format switch
        {
            IndexFormat.UInt32 => IndexType.Uint32,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public override unsafe void SetResourceSet(uint set, ResourceSet resourceSet)
    {
        if (resourceSet is not DescriptorSet descriptorSet)
            throw new Exception("Cannot use non-vulkan resource set with a vulkan command list");

        if (_pipeline is null)
            throw new Exception("A pipeline must be set on the command list before setting a resource set");

        _vk.CmdBindDescriptorSets(SwapChain.CurrentCommandBuffer.VkCommandBuffer, PipelineBindPoint.Graphics,
            _pipeline.VkPipelineLayout, set, 1, descriptorSet.VkDescriptorSet, 0, null);
    }

    public override void DrawIndexed(uint indexCount)
    {
        _vk.CmdDrawIndexed(SwapChain.CurrentCommandBuffer.VkCommandBuffer, indexCount, 1, 0, 0, 0);
    }
}