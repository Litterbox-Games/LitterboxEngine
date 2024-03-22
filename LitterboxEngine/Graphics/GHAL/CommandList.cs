using System.Drawing;
using System.Numerics;

namespace LitterboxEngine.Graphics.GHAL;

public abstract class CommandList
{
    public abstract void Begin(Color clearColor);
    public abstract void End();
    public abstract void SetPipeline(Pipeline pipeline);
    public abstract void SetIndexBuffer(Buffer buffer, IndexFormat format);
    public abstract void SetVertexBuffer(ulong offset, Buffer buffer);
    public abstract void UpdateBuffer<T>(Buffer buffer, ulong offset, T data) where T : unmanaged;
    public abstract void UpdateBuffer<T>(Buffer buffer, ulong offset, T[] data) where T : unmanaged;
    public abstract void SetResourceSet(ResourceSet resourceSet);
    public abstract void DrawIndexed(uint indexCount);
}

// TODO: Add more formats
public enum IndexFormat
{
    UInt32
}