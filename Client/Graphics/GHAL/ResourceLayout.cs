namespace Client.Graphics.GHAL;

public abstract class ResourceLayout
{
    
}

public record ResourceLayoutDescription(params ResourceLayoutElementDescription[] Elements);

public record ResourceLayoutElementDescription(
    ResourceKind Kind,
    ShaderStages Stages,
    uint ArraySize = 1);

public enum ResourceKind
{
    Sampler,
    TextureReadOnly,
    UniformBuffer
}