using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class CommandList: GHAL.CommandList
{

    private readonly Vk _vk;
    private readonly SwapChain _swapChain;

    private Pipeline? _pipeline;

    public CommandList(Vk vk, SwapChain swapChain)
    {
        _vk = vk;
        _swapChain = swapChain;
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

        _pipeline = vulkanPipeline;

        _vk.CmdBindPipeline(_swapChain.CurrentCommandBuffer.VkCommandBuffer, PipelineBindPoint.Graphics, vulkanPipeline.VkPipeline);
        
        // TODO: Revisit this after scissor rect is handled and resizing is handled properly
        Viewport viewport = new()
        {
            X = 0,
            Y = 0,
            Width = _swapChain.Extent.Width,
            Height = _swapChain.Extent.Height,
            MinDepth = 0,
            MaxDepth = 1
        };
            
        Rect2D scissor = new()
        {
            Offset = { X = 0, Y = 0 },
            Extent = _swapChain.Extent
        };

        _vk.CmdSetViewport(_swapChain.CurrentCommandBuffer.VkCommandBuffer, 0, 1, in viewport);
        _vk.CmdSetScissor(_swapChain.CurrentCommandBuffer.VkCommandBuffer, 0, 1, in scissor);
    }

    public override void SetIndexBuffer(GHAL.Buffer buffer, IndexFormat format)
    {
        var indexBuffer = (buffer as Buffer)!.VkBuffer;
        _vk.CmdBindIndexBuffer(_swapChain.CurrentCommandBuffer.VkCommandBuffer, indexBuffer, 0, IndexTypeFromIndexFormat(format));
    }

    public override unsafe void SetVertexBuffer(ulong offset, GHAL.Buffer buffer)
    {
        var vertexBuffers = stackalloc[] { (buffer as Buffer)!.VkBuffer };
        var offsets = stackalloc[] {offset};
        _vk.CmdBindVertexBuffers(_swapChain.CurrentCommandBuffer.VkCommandBuffer, 0, 1, vertexBuffers, offsets);        
    }

    public override void UpdateBuffer<T>(GHAL.Buffer buffer, ulong offset, T data)
    {
        buffer.Update(offset, data);
        // var vkBuffer = (buffer as Buffer)!.VkBuffer;
        // _vk.CmdUpdateBuffer(_swapChain.CurrentCommandBuffer.VkCommandBuffer, vkBuffer, offset, (ulong)Unsafe.SizeOf<T>() ,ref data);
    }

    public override void UpdateBuffer<T>(GHAL.Buffer buffer, ulong offset, T[] data)
    {
        buffer.Update(offset, data);
        // TODO: Revisit the below approach, problem is it cannot be done inside an active render pass
        // var vkBuffer = (buffer as Buffer)!.VkBuffer;
        // var span = new Span<T>(data);
        // _vk.CmdUpdateBuffer(_swapChain.CurrentCommandBuffer.VkCommandBuffer, vkBuffer, offset, span);
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

        _vk.CmdBindDescriptorSets(_swapChain.CurrentCommandBuffer.VkCommandBuffer, PipelineBindPoint.Graphics,
            _pipeline.VkPipelineLayout, set, 1, descriptorSet.VkDescriptorSet, 0, null);
    }

    public override void DrawIndexed(uint indexCount)
    {
        _vk.CmdDrawIndexed(_swapChain.CurrentCommandBuffer.VkCommandBuffer, indexCount, 1, 0, 0, 0);
    }
}