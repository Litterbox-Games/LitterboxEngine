using System.Numerics;
using System.Runtime.InteropServices;

namespace LitterboxEngine.Graphics;

public class Renderer
{
    private const int MaxQuads = 100;
    
    private const int IndicesPerQuad = 6;
    private const int VerticesPerQuad = 4;
    
    private const int MaxVertices = MaxQuads * VerticesPerQuad;
    private const int MaxIndices = MaxQuads * IndicesPerQuad;
    
    private readonly GraphicsDevice _graphicsDevice;

    private readonly Vertex[] _vertices;
    
    public Renderer(GraphicsBackend backend = GraphicsBackend.Vulkan)
    {
        _graphicsDevice = GraphicsDevice.Create(backend);
        
        // Vertex Buffer
        _vertices = new Vertex[MaxVertices];
        // _vertexBuffer = _graphicsDevice.CreateBuffer();

    }
}


// TODO: Implement
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vertex
{
    
}