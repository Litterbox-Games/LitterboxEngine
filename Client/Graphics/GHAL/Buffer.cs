namespace Client.Graphics.GHAL;

public abstract class Buffer : IDisposable
{
    public abstract void Update<T>(ulong offset, T[] data) where T : unmanaged;
    public abstract void Update<T>(ulong offset, T data) where T : unmanaged;
    public abstract void Dispose();
}

public record BufferDescription(
    ulong Size,
    BufferUsage Usage);

public enum BufferUsage
{
    Vertex,
    Index,
    Uniform
}
    