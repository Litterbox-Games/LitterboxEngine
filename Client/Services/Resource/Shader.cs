using Client.Graphics.Backend;
using Common.Services.Resource;

namespace Client.Services.Resource;

public class Shader : IResource<Shader>, IDisposable
{
    public readonly ShaderDescription ShaderDescription;

    private Shader(ShaderDescription description)
    {
        ShaderDescription = description;
    }
    
    public static Shader LoadFromFile(string path)
    {
        var shaderSource = File.ReadAllBytes(path);
        var extension = Path.GetExtension(path);
        
        var shaderType = extension switch
        {
            ".vert" => ShaderStages.Vertex,
            ".frag" => ShaderStages.Fragment,
            _ => throw new Exception($"Unsupported shader file extension {extension}")
        };

        var shaderDescription = new ShaderDescription(shaderType, shaderSource, "main", path);
        
        return new Shader(shaderDescription);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}