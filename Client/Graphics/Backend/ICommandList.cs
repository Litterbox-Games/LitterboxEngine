namespace Client.Graphics.Backend;

public interface ICommandList
{
    public void Begin();
    public void BeginPass();
    public void EndPass();
    public void End();
    public void SetPipeline(Pipeline pipeline);
    public void SetIndexBuffer(Buffer buffer, IndexFormat format);
    public void SetVertexBuffer(ulong offset, Buffer buffer);
    public void UpdateBuffer<T>(Buffer buffer, ulong offset, T data) where T : unmanaged;
    public void UpdateBuffer<T>(Buffer buffer, ulong offset, T[] data) where T : unmanaged;
    public void SetResourceSet(uint set, ResourceSet resourceSet);
    public void DrawIndexed(uint indexCount);
    public void Draw(uint indexCount);
}

public enum IndexFormat
{
    UInt32
}