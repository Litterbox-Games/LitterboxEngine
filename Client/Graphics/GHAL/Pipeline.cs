namespace Client.Graphics.GHAL;

public abstract class Pipeline: IDisposable
{
    public abstract void Dispose();
}

public record PipelineDescription(
    RasterizationStateDescription RasterizationState,
    PrimitiveTopology PrimitiveTopology,
    ResourceLayout[] ResourceLayouts,
    ShaderSetDescription ShaderSet);

public record ShaderSetDescription(
    ShaderProgram ShaderProgram,
    VertexLayoutDescription VertexLayout);

#region VertexInputState
public record VertexLayoutDescription
{
    public readonly uint Stride;
    public readonly uint Binding;
    public readonly VertexElementDescription[] ElementDescriptions;

    public VertexLayoutDescription(params VertexElementDescription[] elementDescriptions): 
        this(0, elementDescriptions) {}
    
    private VertexLayoutDescription(uint binding = 0, params VertexElementDescription[] elementDescriptions)
    {
        // Calculate offsets for elements
        uint offset = 0;
        foreach (var e in elementDescriptions)
        {
            e.Offset = offset;
            offset += e.Format.SizeOf();
        }

        Stride = offset;
        Binding = binding;
        ElementDescriptions = elementDescriptions;
    }
}

public record VertexElementDescription(uint Location, VertexElementFormat Format, uint Binding = 0, uint Offset = 0)
{
    // Modified by VertexLayoutDescription
    public uint Offset = Offset;
}

public enum VertexElementFormat
{
    Float,
    Float2,
    Float3,
    Float4,
    Int
}

public static class VertexElementFormatExtensions
{
    public static uint SizeOf(this VertexElementFormat format)
    {
        return format switch
        {
            VertexElementFormat.Float => 4,
            VertexElementFormat.Float2 => 8,
            VertexElementFormat.Float3 => 12,
            VertexElementFormat.Float4 => 16,
            VertexElementFormat.Int => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}
#endregion

#region RasterizationState
public record RasterizationStateDescription(
    CullMode CullMode,
    FillMode FillMode,
    FrontFace FrontFace,
    bool EnableScissor,
    // If EnableDepthTest is false, fragments outside of depth range are clamped to closest value
    // If true, fragments outside of depth range are discarded automatically
    bool EnableDepthTest,
    float LineWidth = 1);

public enum CullMode
{
    None,
    Front,
    Back,
    FrontAndBack
}

public enum FillMode
{
    Solid,
    Line,
    Point
}

public enum FrontFace
{
    ClockWise,
    CounterClockWise
}
#endregion

#region InputAssemblyState
public enum PrimitiveTopology
{
    TriangleList,
    LineList,
    LineStrip,
    PatchList, 
    PointList,
    TriangleFan,
    TriangleStrip,
    LineListWithAdjacency,
    LineStripWithAdjacency,
    TriangleListWithAdjacency,
    TriangleStripWithAdjacency
}
#endregion






