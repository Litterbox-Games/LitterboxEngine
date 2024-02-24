namespace LitterboxEngine.Graphics;

public class Pipeline
{
    
}

public record PipelineDescription
{ 
    public ShaderProgram ShaderProgram;

    public PipelineDescription(ShaderProgram shaderProgram)
    {
        ShaderProgram = shaderProgram;
    }
}