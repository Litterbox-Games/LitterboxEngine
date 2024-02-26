using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LitterboxEngine.Graphics;

public abstract class Pipeline: IDisposable
{
    public abstract void Dispose();
}

public record PipelineDescription(
    RasterizationStateDescription RasterizationState,
    PrimitiveTopology PrimitiveTopology,
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

    private VertexLayoutDescription(uint stride, VertexElementDescription[] elementDescriptions, uint binding)
    {
        Stride = stride;
        Binding = binding;
        ElementDescriptions = elementDescriptions;
    }
    
    public static VertexLayoutDescription New<T>(params VertexElementDescriptionCreateInfo[] elements)
    {
        return new VertexLayoutDescription(
            (uint) Unsafe.SizeOf<T>(),
            elements
                .Select(VertexElementDescription.New<T>)
                .ToArray(),
                0);
    }
}

// TODO: possibly switch names with VertexElementDescription?
public record VertexElementDescriptionCreateInfo(
    uint Location, 
    string Name, 
    VertexElementFormat Format,
    uint Binding = 0);

public record VertexElementDescription
{
    public readonly uint Location;
    public readonly uint Offset;
    public readonly uint Binding;
    public readonly VertexElementFormat Format;

    private VertexElementDescription(uint location, uint offset, VertexElementFormat format, uint binding)
    {
        Location = location;
        Offset = offset;
        Binding = binding;
        Format = format;
    }
    
    public static VertexElementDescription New<T>(VertexElementDescriptionCreateInfo info)
    {
        var offset = (uint)Marshal.OffsetOf<T>(info.Name);
        return new VertexElementDescription(info.Location, offset, info.Format, info.Binding);
    }
}

/*
public record VertexElementDescription(
    uint Location,
    string Name,
    //uint Offset,
    VertexElementFormat Format,
    uint Binding = 0);
*/
public enum VertexElementFormat
{
    // TODO: Fill with values equivalent to Silk.NET.Vulkan.Format
    Float1,
    Float2,
    Float3,
    Float4
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






