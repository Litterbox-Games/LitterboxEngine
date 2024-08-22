using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Client.Graphics.GHAL;
using Client.Resource;
using Common.Resource;
using Buffer = Client.Graphics.GHAL.Buffer;

namespace Client.Graphics;

public class RendererService: IRendererService
{
    private const int MaxQuads = 100000;
    private const int MaxTextures = 8;
    
    private const int IndicesPerQuad = 6;
    
    private uint _quadCount;
    private uint IndexCount => _quadCount * IndicesPerQuad;
    
    private int _textureCount = 1;

    private readonly IGraphicsDeviceService _graphicsDeviceService;
    private readonly Pipeline _pipeline;
    private readonly CommandList _commandList;

    private readonly Quad[] _quads;
    
    private readonly Buffer _quadsBuffer;
    private readonly ResourceSet _quadsSet;

    private readonly Buffer _transformBuffer;
    private readonly ResourceSet _transformSet;
    
    private readonly Sampler _sampler;
    private readonly Texture _whiteTexture;
    private readonly Texture[] _textures;
    private readonly ResourceSet _textureSet;
    
    private Matrix4x4 _mvp;
    
    public Color ClearColor { get; set; } = Color.Black;
    
    public unsafe RendererService(IResourceService resourceService, IGraphicsDeviceService graphicsDeviceService)
    {
        _graphicsDeviceService = graphicsDeviceService;

        _quads = new Quad[MaxQuads];
        
        var vertexShaderDesc = resourceService.Get<Shader>("Shaders/default.vert").ShaderDescription;
        var fragmentShaderDesc = resourceService.Get<Shader>("Shaders/default.frag").ShaderDescription;

        using var shaderProgram = _graphicsDeviceService.CreateShaderProgram(vertexShaderDesc, fragmentShaderDesc);

        // default.vert:15-17
        // layout(set = 0, binding = 0) uniform MVPBuffer {
        //     mat4 uMVP;
        // };
        _transformBuffer = _graphicsDeviceService.CreateBuffer(new BufferDescription((uint)sizeof(Matrix4x4), BufferUsage.Uniform));
        var transformLayout = _graphicsDeviceService.CreateResourceLayout(
            new ResourceLayoutDescription(new ResourceLayoutElementDescription(ResourceKind.UniformBuffer, ShaderStages.Vertex)));                                                                       
        
        _transformSet = _graphicsDeviceService.CreateResourceSet(transformLayout);
        _transformSet.Update(0, _transformBuffer);
        
        Console.WriteLine(sizeof(Quad));
        
        // default.vert:20-22
        // layout(std140, set = 1, binding = 0) buffer QuadBlock {
        //     Quad quads[];
        // };
        _quadsBuffer = _graphicsDeviceService.CreateBuffer(new BufferDescription((uint)sizeof(Quad) * MaxQuads, BufferUsage.StorageBuffer));
        var quadsLayout = _graphicsDeviceService.CreateResourceLayout(
            new ResourceLayoutDescription(new ResourceLayoutElementDescription(ResourceKind.StorageBuffer, ShaderStages.Vertex)));
        
        _quadsSet = _graphicsDeviceService.CreateResourceSet(quadsLayout);
        _quadsSet.UpdateStorageBuffer(0, _quadsBuffer);
        
        // default.frag:3-4
        // layout(set = 2, binding = 0) uniform texture2D textures[8];
        // layout(set = 2, binding = 1) uniform sampler samp;
        var textureLayout = _graphicsDeviceService.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(ResourceKind.TextureReadOnly, ShaderStages.Fragment, MaxTextures),
                new ResourceLayoutElementDescription(ResourceKind.Sampler, ShaderStages.Fragment)));

        _textureSet = _graphicsDeviceService.CreateResourceSet(textureLayout);
        _textures = new Texture[MaxTextures];
        _whiteTexture = _graphicsDeviceService.CreateTexture(1, 1, Color.White);
        _textures[0] = _whiteTexture;
        for (uint i = 0; i < MaxTextures; i++)
            _textureSet.Update(0, _whiteTexture, i);
            
        _sampler = _graphicsDeviceService.CreateSampler();
        _textureSet.Update(1, _sampler);

        var pipelineDescription = new PipelineDescription(
            RasterizationState: new RasterizationStateDescription(
                CullMode: CullMode.Back,
                FillMode: FillMode.Solid,
                FrontFace: FrontFace.ClockWise,
                EnableScissor: true,
                EnableDepthTest: true),
            PrimitiveTopology: PrimitiveTopology.TriangleList,
            ResourceLayouts: new []{ transformLayout, quadsLayout, textureLayout },
            ShaderSet: new ShaderSetDescription(
                ShaderProgram: shaderProgram,
                // TODO: Make VertexLayout nullable
                VertexLayout: Quad.VertexLayout)
        );

        _pipeline = _graphicsDeviceService.CreatePipeline(pipelineDescription);

        _commandList = _graphicsDeviceService.CreateCommandList();
    }

    public void Begin(Matrix4x4? mvp = null)
    {
        _mvp = mvp ?? Matrix4x4.Identity;
        
        _graphicsDeviceService.SwapBuffers();
        _commandList.Begin(ClearColor);
        _commandList.SetPipeline(_pipeline);
        
        _commandList.UpdateBuffer(_transformBuffer, 0, _mvp);
        _commandList.SetResourceSet(0, _transformSet);
    }

    private void Flush()
    {
        _commandList.SetResourceSet(2, _textureSet);
        
        _commandList.UpdateBuffer(_quadsBuffer, 0, _quads);
        _commandList.SetResourceSet(1, _quadsSet);
        
        _commandList.Draw(IndexCount);

        _quadCount = 0;
        _textureCount = 1;
    }
    
    public void End()
    {
        if (_quadCount > 0) Flush();
        
        _commandList.End();
        _graphicsDeviceService.SubmitCommands();
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

        _quads[_quadCount] = new Quad
        {
            Max = new Vector2(destination.Right, destination.Bottom),
            Min = new Vector2(destination.Left, destination.Top),
            TexMax = new Vector2((float) source.Right / texture.Width, (float) source.Bottom / texture.Height),
            TexMin = new Vector2((float) source.Left / texture.Width, (float) source.Top / texture.Height),
            Color = color,
            Depth = depth,
            TexIndex = texIndex
        };
    }

    public void Dispose()
    {
        _whiteTexture.Dispose();
        _sampler.Dispose();
        _transformBuffer.Dispose();
        _quadsBuffer.Dispose();
        _pipeline.Dispose();
        GC.SuppressFinalize(this);
    }
}

[StructLayout(LayoutKind.Sequential, Size = 64)]
public struct Quad
{
    public required Vector2 Min;
    public required Vector2 Max;
    public required Vector2 TexMin;
    public required Vector2 TexMax;
    public required RgbaFloat Color;
    public required float Depth;
    public required int TexIndex;
    
    public static readonly VertexLayoutDescription VertexLayout = new ();
}