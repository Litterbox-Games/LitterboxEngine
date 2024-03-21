using System.Drawing;

namespace LitterboxEngine.Graphics.GHAL;

public abstract class CommandList
{
    public abstract void Begin(Color clearColor);
    public abstract void End();
    public abstract void SetPipeline(Pipeline pipeline);
    public abstract void SetIndexBuffer();
    public abstract void SetVertexBuffer();
    public abstract void UpdateBuffer();
    public abstract void SetResourceSet(ResourceSet resourceSet);
    public abstract void DrawIndexed();
}