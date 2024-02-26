using System.Text;
using LitterboxEngine.Resource;

namespace LitterboxEngine.Graphics.GHAL.Resources;

public class Shader : IResource, IDisposable
{
    public ShaderDescription ShaderDescription { get; }

    private Shader(ShaderDescription description)
    {
        ShaderDescription = description;
    }
    
    public static IResource LoadFromFile(string path)
    {
        var shaderSource = File.ReadAllText(path);
        var extension = Path.GetExtension(path);
        
        var shaderType = extension switch
        {
            ".vert" => ShaderStages.Vertex,
            ".frag" => ShaderStages.Fragment,
            _ => throw new Exception($"Unsupported shader file extension {extension}")
        };

        var shaderDescription = new ShaderDescription(shaderType, Encoding.UTF8.GetBytes(shaderSource), "main", path);
        
        return new Shader(shaderDescription);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}