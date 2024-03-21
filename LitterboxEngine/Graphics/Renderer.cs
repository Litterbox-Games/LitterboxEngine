using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using LitterboxEngine.Graphics.GHAL;
using LitterboxEngine.Graphics.Resources;
using LitterboxEngine.Resource;
using Buffer = LitterboxEngine.Graphics.GHAL.Buffer;

namespace LitterboxEngine.Graphics;

public class Renderer: IDisposable
{
    
    private const int MaxQuads = 100;
    
    private const int IndicesPerQuad = 6;
    private const int VerticesPerQuad = 4;
    
    private const int MaxVertices = MaxQuads * VerticesPerQuad;
    private const int MaxIndices = MaxQuads * IndicesPerQuad;
    
    private uint _quadCount = 0;
    private uint _vertexCount => _quadCount * VerticesPerQuad;
    private uint _indexCount => _quadCount * IndicesPerQuad;
    
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Pipeline _pipeline;
    private readonly CommandList _commandList;

    private readonly Vertex[] _vertices;
    
    private readonly Buffer _vertexBuffer;
    private readonly Buffer _indexBuffer;

    private readonly Buffer _transformBuffer;
    
    public Color ClearColor = Color.Magenta;
    
    public unsafe Renderer(Window window, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        
        // Vertex Buffer
        _vertices = new Vertex[MaxVertices];
        _vertexBuffer = _graphicsDevice.CreateBuffer(new BufferDescription(MaxVertices * Vertex.VertexLayout.Stride, BufferUsage.Vertex));
        
        // Index Buffer
        var indicesTemplate = new ushort[]
        {
            // Since indices are read clock wise:
            0, 1, 2, // tri 1
            3, 2, 1 // tri 2
        };
    
        var indices = new uint[MaxIndices];
    
        for (var i = 0; i < MaxQuads; i++)
        {
            var startIndex = i * IndicesPerQuad;
            var offset = i * VerticesPerQuad;
    
            indices[startIndex + 0] = (uint)(indicesTemplate[0] + offset);
            indices[startIndex + 1] = (uint)(indicesTemplate[1] + offset);
            indices[startIndex + 2] = (uint)(indicesTemplate[2] + offset);
    
            indices[startIndex + 3] = (uint)(indicesTemplate[3] + offset);
            indices[startIndex + 4] = (uint)(indicesTemplate[4] + offset);
            indices[startIndex + 5] = (uint)(indicesTemplate[5] + offset);
        }
        
        _indexBuffer = _graphicsDevice.CreateBuffer(new BufferDescription(MaxIndices * sizeof(uint), BufferUsage.Index));
        _graphicsDevice.UpdateBuffer(_indexBuffer, 0, indices);
        
        var vertexShaderDesc = ResourceManager.Get<Shader>("Shaders/default.vert").ShaderDescription;
        var fragmentShaderDesc = ResourceManager.Get<Shader>("Shaders/default.frag").ShaderDescription;

        using var shaderProgram = _graphicsDevice.CreateShaderProgram(vertexShaderDesc, fragmentShaderDesc);

        _transformBuffer = _graphicsDevice.CreateBuffer(new BufferDescription(2 * (uint)sizeof(Matrix4x4), BufferUsage.Uniform));
        var transformLayout = _graphicsDevice.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "ProjectionViewBuffer", 
                    ResourceKind.UniformBuffer, 
                    ShaderStages.Vertex)));                                                                       
        
        //_transformSet = _graphicsDevice.CreateResourceSet(new ResourceSetDescription(transformLayout, _transformBuffer));

        var textureLayout = _graphicsDevice.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("tex0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("sampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        
        var pipelineDescription = new PipelineDescription(
            RasterizationState: new RasterizationStateDescription(
                CullMode: CullMode.Back,
                FillMode: FillMode.Solid,
                FrontFace: FrontFace.ClockWise,
                EnableScissor: true,
                EnableDepthTest: true),
            PrimitiveTopology: PrimitiveTopology.TriangleList,
            ResourceLayouts: new []{ transformLayout, textureLayout },
            ShaderSet: new ShaderSetDescription(
                ShaderProgram: shaderProgram,
                VertexLayout: Vertex.VertexLayout)
        );

        _pipeline = _graphicsDevice.CreatePipeline(pipelineDescription);

        _commandList = _graphicsDevice.CreateCommandList();
    }

    public void Begin()
    {
        _graphicsDevice.SwapBuffers();
        _commandList.Begin(ClearColor);
        // _commandList.SetPipeline(_pipeline);
        // _commandList.SetIndexBuffer();
    }

    private void Flush()
    {
        _commandList.UpdateBuffer(); // Update vertex buffer
        _commandList.SetVertexBuffer();
        
        _commandList.DrawIndexed();
    }
    
    public void End()
    {
        // Flush();
        
        _commandList.End();
        _graphicsDevice.SubmitCommands();
    }
    
    private void AddQuad(Vertex topLeft, Vertex topRight, Vertex bottomLeft, Vertex bottomRight)
    {
        // Flush the vertex buffer if its full
        if (_vertexCount >= MaxVertices) Flush();
        
        _vertices[_vertexCount] = topLeft;
        _vertices[_vertexCount + 1] = topRight;
        _vertices[_vertexCount + 2] = bottomLeft;
        _vertices[_vertexCount + 3] = bottomRight;

        _quadCount++;
    }

    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _pipeline.Dispose();
        GC.SuppressFinalize(this);
    }
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vertex
{
    public required Vector3 Position { get; init; }
    public required Color Color { get; init; }
    public required Vector2 TexCoords { get; init; }

    public static readonly VertexLayoutDescription VertexLayout = new (
        new VertexElementDescription(0, VertexElementFormat.Float3),
        new VertexElementDescription(1, VertexElementFormat.Float4),
        new VertexElementDescription(2, VertexElementFormat.Float2)
    );
}