namespace LitterboxEngine.Graphics.GHAL;

public class ResourceLayout
{
    
}

public record ResourceLayoutDescription(params ResourceLayoutElementDescription[] Elements);

public record ResourceLayoutElementDescription(
    string Name,
    ResourceKind Kind,
    ShaderStages Stages);

public enum ResourceKind
{
    Sampler,
    TextureReadOnly,
    UniformBuffer
}