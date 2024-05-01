namespace Client.Graphics.GHAL;

public abstract class CommandList
{
    public abstract void Begin(RgbaFloat clearColor);
    public abstract void End();
    public abstract void SetPipeline(Pipeline pipeline);
    public abstract void SetIndexBuffer(Buffer buffer, IndexFormat format);
    public abstract void SetVertexBuffer(ulong offset, Buffer buffer);
    public abstract void UpdateBuffer<T>(Buffer buffer, ulong offset, T data) where T : unmanaged;
    public abstract void UpdateBuffer<T>(Buffer buffer, ulong offset, T[] data) where T : unmanaged;
    public abstract void SetResourceSet(uint set, ResourceSet resourceSet);
    public abstract void DrawIndexed(uint indexCount);
}

public enum IndexFormat
{
    UInt32
}