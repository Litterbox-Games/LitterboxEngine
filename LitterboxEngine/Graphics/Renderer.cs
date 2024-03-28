using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using LitterboxEngine.Graphics.GHAL;
using LitterboxEngine.Graphics.Resources;
using LitterboxEngine.Resource;
using Buffer = LitterboxEngine.Graphics.GHAL.Buffer;

namespace LitterboxEngine.Graphics;

// TODO: List
// 1. Handle window resizing
// 2. Go through all leftover TODOs in other files


public class Renderer: IDisposable
{
    
    private const int MaxQuads = 100;
    private const int MaxTextures = 8;
    
    private const int IndicesPerQuad = 6;
    private const int VerticesPerQuad = 4;
    
    private const int MaxVertices = MaxQuads * VerticesPerQuad;
    private const int MaxIndices = MaxQuads * IndicesPerQuad;
    
    private uint _quadCount;
    private uint VertexCount => _quadCount * VerticesPerQuad;
    private uint IndexCount => _quadCount * IndicesPerQuad;
    
    private int _textureCount = 1;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly Pipeline _pipeline;
    private readonly CommandList _commandList;

    private readonly Vertex[] _vertices;
    
    private readonly Buffer _vertexBuffer;
    private readonly Buffer _indexBuffer;

    private readonly Buffer _transformBuffer;
    private readonly ResourceSet _transformSet;
    
    private readonly Sampler _sampler;
    private readonly Texture _whiteTexture;
    private readonly Texture[] _textures;
    private readonly ResourceSet _textureSet;
    
    private Matrix4x4 _projection;
    private Matrix4x4 _view;
    
    public Color ClearColor = Color.Black;
    
    public unsafe Renderer(Window window, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        
        // Initialize projection matrix
        SetViewSize(new Vector2(window.Width, window.Height));
        
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
            // TODO: Rename ResourceLayoutElementDescription to Binding and remove ResourceLayoutDescription
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ResourceKind.UniformBuffer, ShaderStages.Vertex)));                                                                       
        
        _transformSet = _graphicsDevice.CreateResourceSet(transformLayout);
        _transformSet.Update(0, _transformBuffer);

        var textureLayout = _graphicsDevice.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ResourceKind.TextureReadOnly, ShaderStages.Fragment, MaxTextures),
                new ResourceLayoutElementDescription(ResourceKind.Sampler, ShaderStages.Fragment)));

        _textureSet = _graphicsDevice.CreateResourceSet(textureLayout);
        _textures = new Texture[MaxTextures];
        _whiteTexture = _graphicsDevice.CreateTexture(1, 1, Color.White);
        _textures[0] = _whiteTexture;
        for (uint i = 0; i < MaxTextures; i++)
            _textureSet.Update(0, _whiteTexture, i);
            
        _sampler = _graphicsDevice.CreateSampler();
        _textureSet.Update(1, _sampler);

        var pipelineDescription = new PipelineDescription(
            RasterizationState: new RasterizationStateDescription(
                CullMode: CullMode.Back,
                FillMode: FillMode.Solid,
                FrontFace: FrontFace.ClockWise,
                EnableScissor: true,
                EnableDepthTest: true),
            PrimitiveTopology: PrimitiveTopology.TriangleList,
            ResourceLayouts: new []{ transformLayout , textureLayout },
            ShaderSet: new ShaderSetDescription(
                ShaderProgram: shaderProgram,
                VertexLayout: Vertex.VertexLayout)
        );

        _pipeline = _graphicsDevice.CreatePipeline(pipelineDescription);

        _commandList = _graphicsDevice.CreateCommandList();
    }

    public void SetViewSize(Vector2 size)
    {   
       _projection = Matrix4x4.CreateTranslation(-size.X / 2f, -size.Y / 2f, 0) 
                     * Matrix4x4.CreateScale(1 / (size.X / 2f), 1 / (size.Y / 2f), 1);
    }
    
    public unsafe void Begin(Matrix4x4? view = null)
    {
        _view = view ?? Matrix4x4.Identity;
        
        _graphicsDevice.SwapBuffers();
        _commandList.Begin(ClearColor);
        _commandList.SetPipeline(_pipeline);
        
        
        
        _commandList.UpdateBuffer(_transformBuffer, 0, _projection);
        _commandList.UpdateBuffer(_transformBuffer, (ulong)sizeof(Matrix4x4), _view);
        _commandList.SetResourceSet(0, _transformSet);
    }

    private void Flush()
    {
        _commandList.SetResourceSet(1, _textureSet);
        
        _commandList.UpdateBuffer(_vertexBuffer, 0, _vertices);
        _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
        _commandList.SetVertexBuffer(0, _vertexBuffer);

        _commandList.DrawIndexed(IndexCount);

        _quadCount = 0;
        _textureCount = 1;
    }
    
    public void End()
    {
        Flush();
        
        _commandList.End();
        _graphicsDevice.SubmitCommands();
    }

    public void DrawRectangle(RectangleF destination, Color color, float depth = 0.0f)
    {
        DrawTexture(_whiteTexture, destination, color, depth);
    }

    public void DrawTexture(Texture texture, RectangleF destination, Color color, float depth = 0.0f)
    {
        DrawTexture(texture, new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), destination, color, depth);
    }
    
    public void DrawTexture(Texture texture, Rectangle source, RectangleF destination, Color color, float depth = 0.0f /* depth should be in the range [-1, 0] */)
    {
        var texIndex = Array.IndexOf(_textures, texture, 0, _textureCount);
        
        if (texIndex == -1) // Current texture is not in array
        {
            // Flush textures if texture array is full
            if (_textureCount >= MaxTextures) Flush();
            texIndex = _textureCount;
            _textures[texIndex] = texture;
            _textureSet.Update(0, texture, (uint)texIndex);
            _textureCount++;
        }
        
        AddQuad(
            new Vertex { Position = new Vector3(destination.Left, destination.Top, depth), Color = color, TexCoords = new Vector2((float)source.Left / texture.Width, (float)source.Top / texture.Height), TexIndex = texIndex },
            new Vertex { Position = new Vector3(destination.Right, destination.Top, depth), Color = color, TexCoords = new Vector2((float)source.Right / texture.Width, (float)source.Top / texture.Height), TexIndex = texIndex },
            new Vertex { Position = new Vector3(destination.Left, destination.Bottom, depth), Color = color, TexCoords = new Vector2((float)source.Left / texture.Width, (float)source.Bottom / texture.Height), TexIndex = texIndex },
            new Vertex { Position = new Vector3(destination.Right, destination.Bottom, depth), Color = color, TexCoords = new Vector2((float)source.Right / texture.Width, (float)source.Bottom / texture.Height), TexIndex = texIndex }
        );
    }
    
    private void AddQuad(Vertex topLeft, Vertex topRight, Vertex bottomLeft, Vertex bottomRight)
    {
        // Flush the vertex buffer if its full
        if (VertexCount >= MaxVertices) Flush();
        
        _vertices[VertexCount] = topLeft;
        _vertices[VertexCount + 1] = topRight;
        _vertices[VertexCount + 2] = bottomLeft;
        _vertices[VertexCount + 3] = bottomRight;

        _quadCount++;
    }

    public void Dispose()
    {
        _whiteTexture.Dispose();
        _sampler.Dispose();
        _transformBuffer.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _pipeline.Dispose();
        GC.SuppressFinalize(this);
    }
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vertex
{
    public required Vector3 Position;
    public required RgbaFloat Color;
    public required Vector2 TexCoords;
    public required int TexIndex;

    public static readonly VertexLayoutDescription VertexLayout = new (
        new VertexElementDescription(0, VertexElementFormat.Float3),
        new VertexElementDescription(1, VertexElementFormat.Float4),
        new VertexElementDescription(2, VertexElementFormat.Float2),
        new VertexElementDescription(3, VertexElementFormat.Int)
    );
}