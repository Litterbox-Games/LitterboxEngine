namespace LitterboxEngine.Graphics.GHAL;

public abstract class CommandList
{
    public abstract void Begin();
    public abstract void End();
    public abstract void SetFrameBuffer();
    // public abstract void ClearColorTarget();
    // public abstract void ClearDepthStencil();
    public abstract void SetPipeline();
    public abstract void SetIndexBuffer();
    public abstract void UpdateBuffer();
    // public abstract void SetGraphicsResourceSet();
    public abstract void SetVertexBuffer();
    public abstract void DrawIndexed();
}