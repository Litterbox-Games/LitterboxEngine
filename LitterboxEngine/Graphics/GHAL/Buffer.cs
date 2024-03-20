namespace LitterboxEngine.Graphics.GHAL;

public abstract class Buffer : IDisposable
{
    public abstract void Update(uint offset, uint[] data);
    public abstract void Dispose();
}

public record BufferDescription(
    uint Size,
    BufferUsage Usage);

public enum BufferUsage
{
    Vertex,
    Index
}
    