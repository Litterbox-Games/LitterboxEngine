using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using LitterboxEngine.Graphics.GHAL;
using LitterboxEngine.Graphics.Resources;
using LitterboxEngine.Resource;

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
    
    public Renderer(Window window, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        
        // Vertex Buffer
        _vertices = new Vertex[MaxVertices];
        // _vertexBuffer = _graphicsDevice.CreateBuffer();
        
        /*
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
            ShaderSet: new ShaderSetDescription(
                ShaderProgram: shaderProgram,
                VertexLayout: Vertex.VertexLayout)
        );

        _pipeline = _graphicsDevice.CreatePipeline(pipelineDescription);

        _commandList = _graphicsDevice.CreateCommandList();
        */
    }

    public void Begin()
    {
        _graphicsDevice.SwapBuffers();
        // _commandList.Begin();
        // _commandList.SetClearColor(Color.Black);
        // _commandList.SetPipeline();
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
        // _pipeline.Dispose();
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