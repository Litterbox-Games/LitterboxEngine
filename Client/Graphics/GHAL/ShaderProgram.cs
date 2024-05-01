namespace Client.Graphics.GHAL;

public abstract class ShaderProgram: IDisposable
{
    public abstract void Dispose();
}

public enum ShaderStages
{
    Vertex,
    Fragment
}

public record ShaderDescription(ShaderStages ShaderStage, byte[] Source, string EntryPoint, string Path);