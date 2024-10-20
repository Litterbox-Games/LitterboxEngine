using Silk.NET.Vulkan;

namespace Client.Graphics.GHAL.Vulkan;

public class VulkanCommandList: CommandList
{

    private readonly Vk _vk;
    private readonly VulkanSwapChain _swapChain;
    private readonly VulkanRenderPass _renderPass;

    private VulkanPipeline? _pipeline;

    public VulkanCommandList(Vk vk, VulkanSwapChain swapChain, VulkanRenderPass renderPass)
    {
        _vk = vk;
        _swapChain = swapChain;
        _renderPass = renderPass;
    }

    public override void Begin()
    {
        _swapChain.CurrentCommandBuffer.BeginRecording();
    }

    public override unsafe void BeginRenderPass(RgbaFloat clearColor)
    {
        ClearValue clearValue = new()
        {
            Color = new ClearColorValue
                { Float32_0 = clearColor.R, Float32_1 = clearColor.G, Float32_2 = clearColor.B, Float32_3 = 1 },
        };
        
        RenderPassBeginInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _renderPass.VkRenderPass,
            Framebuffer = _swapChain.CurrentFrameBuffer.VkFrameBuffer,
            RenderArea =
            {
                Offset = { X = 0, Y = 0 },
                Extent = _swapChain.Extent
            },
            ClearValueCount = 1,
            PClearValues = &clearValue
        };
        
        _vk.CmdBeginRenderPass(_swapChain.CurrentCommandBuffer.VkCommandBuffer, &renderPassInfo, SubpassContents.Inline);
    }

    public override void EndRenderPass()
    {
        _vk.CmdEndRenderPass(_swapChain.CurrentCommandBuffer.VkCommandBuffer);
    }

    public override void End()
    {
        _swapChain.CurrentCommandBuffer.EndRecording();
    }

    public override void SetPipeline(GHAL.Pipeline pipeline)
    {
        if (pipeline is not VulkanPipeline vulkanPipeline)
            throw new Exception("Cannot use non-vulkan pipeline with a vulkan command list");

        _pipeline = vulkanPipeline;

        _vk.CmdBindPipeline(_swapChain.CurrentCommandBuffer.VkCommandBuffer, PipelineBindPoint.Graphics, vulkanPipeline.VkPipeline);
        
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

    public override void SetIndexBuffer(Buffer buffer, IndexFormat format)
    {
        var indexBuffer = (buffer as VulkanBuffer)!.VkBuffer;
        _vk.CmdBindIndexBuffer(_swapChain.CurrentCommandBuffer.VkCommandBuffer, indexBuffer, 0, IndexTypeFromIndexFormat(format));
    }

    public override unsafe void SetVertexBuffer(ulong offset, Buffer buffer)
    {
        var vertexBuffers = stackalloc[] { (buffer as VulkanBuffer)!.VkBuffer };
        var offsets = stackalloc[] {offset};
        _vk.CmdBindVertexBuffers(_swapChain.CurrentCommandBuffer.VkCommandBuffer, 0, 1, vertexBuffers, offsets);        
    }

    public override void UpdateBuffer<T>(Buffer buffer, ulong offset, T data)
    {
        buffer.Update(offset, data);
    }

    public override void UpdateBuffer<T>(Buffer buffer, ulong offset, T[] data)
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
        if (resourceSet is not VulkanDescriptorSet descriptorSet)
            throw new Exception("Cannot use non-vulkan resource set with a vulkan command list");

        if (_pipeline is null)
            throw new Exception("A pipeline must be set on the command list before setting a resource set");

        _vk.CmdBindDescriptorSets(_swapChain.CurrentCommandBuffer.VkCommandBuffer, PipelineBindPoint.Graphics,
            _pipeline.VkPipelineLayout, set, 1, in descriptorSet.VkDescriptorSet, 0, null);
    }

    public override void DrawIndexed(uint indexCount)
    {
        _vk.CmdDrawIndexed(_swapChain.CurrentCommandBuffer.VkCommandBuffer, indexCount, 1, 0, 0, 0);
    }

    public override void Draw(uint indexCount)
    {
        _vk.CmdDraw(_swapChain.CurrentCommandBuffer.VkCommandBuffer, indexCount, 1, 0, 0);        
    }
}