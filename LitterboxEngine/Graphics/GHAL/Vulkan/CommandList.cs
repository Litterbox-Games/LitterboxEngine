using Silk.NET.Vulkan;

namespace LitterboxEngine.Graphics.GHAL.Vulkan;

public class CommandList: GHAL.CommandList
{

    private readonly Vk _vk;
    
    public CommandList(Vk vk)
    {
        _vk = vk;
        
        
        
        
        
        
        
        
    }
    
    public override void Begin()
    {
        // Wait For Fences
        
        // _vk.BeginCommandBuffer();
        // _vk.CmdBeginRenderPass();



        throw new NotImplementedException();
    }

    public override void End()
    {
        throw new NotImplementedException();
    }

    public override void SetFrameBuffer()
    {
        throw new NotImplementedException();
    }

    public override void SetPipeline()
    {
        // Called after Begin()
        // _vk.CmdBindPipeline()



        throw new NotImplementedException();
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