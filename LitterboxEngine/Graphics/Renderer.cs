using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using LitterboxEngine.Resource;

namespace LitterboxEngine.Graphics;

public class Renderer: IDisposable
{
    private const int MaxQuads = 100;
    
    private const int IndicesPerQuad = 6;
    private const int VerticesPerQuad = 4;
    
    private const int MaxVertices = MaxQuads * VerticesPerQuad;
    private const int MaxIndices = MaxQuads * IndicesPerQuad;
    
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Pipeline _pipeline;

    private readonly Vertex[] _vertices;
    
    public Renderer(Window window, GraphicsBackend backend = GraphicsBackend.Vulkan)
    {
        var description = new GraphicsDeviceDescription();
        
        _graphicsDevice = GraphicsDevice.Create(window, description, backend);
        
        // Vertex Buffer
        _vertices = new Vertex[MaxVertices];
        // _vertexBuffer = _graphicsDevice.CreateBuffer();
        
        var vertexShaderDesc = ResourceManager.Get<Shader>("Shaders/default.vert").ShaderDescription;
        var fragmentShaderDesc = ResourceManager.Get<Shader>("Shaders/default.frag").ShaderDescription;

        var shaderProgram = _graphicsDevice.CreateShaderProgram(vertexShaderDesc, fragmentShaderDesc);

        var pipelineDescription = new PipelineDescription(
            RasterizationState: new RasterizationStateDescription(
                CullMode: CullMode.Back,
                FillMode: FillMode.Solid,
                FrontFace: FrontFace.ClockWise,
                EnableScissor: true,
                EnableDepthTest: true),
            PrimitiveTopology: PrimitiveTopology.TriangleList,
            // ResourceLayouts: idk, //TODO: implement this when we start using Uniforms Buffers
            ShaderSet: new ShaderSetDescription(
                ShaderProgram: shaderProgram,
                VertexLayout: Vertex.VertexLayout)
        );

        _pipeline = _graphicsDevice.CreatePipeline(pipelineDescription);
    }

    public void Dispose()
    {
        _pipeline.Dispose();
        _graphicsDevice.Dispose();
        GC.SuppressFinalize(this);
    }
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vertex
{
    public Vector3 Position;
    public Color Color;
    public Vector2 TexCoords;

    public static readonly VertexLayoutDescription VertexLayout = VertexLayoutDescription.New<Vertex>(
        new VertexElementDescriptionCreateInfo(0, nameof(Position), VertexElementFormat.Float3),
        new VertexElementDescriptionCreateInfo(1, nameof(Color), VertexElementFormat.Float4),
        new VertexElementDescriptionCreateInfo(2, nameof(TexCoords), VertexElementFormat.Float2)
    );
}