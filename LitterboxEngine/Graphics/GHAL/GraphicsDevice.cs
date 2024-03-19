namespace LitterboxEngine.Graphics.GHAL;

public abstract class GraphicsDevice: IDisposable
{
    public static GraphicsDevice Create(Window window, GraphicsDeviceDescription description, GraphicsBackend backend)
    {
        return backend switch
        {
            GraphicsBackend.Vulkan => CreateVulkanGraphicsDevice(window, description),
            _ => throw new NotImplementedException($"A GraphicsDevice for {backend} has not been implemented")
        };
    }
    
    private static Vulkan.GraphicsDevice CreateVulkanGraphicsDevice(Window window, GraphicsDeviceDescription description)
    {
        return new Vulkan.GraphicsDevice(window, description);
    }

    public abstract void CreateBuffer();
    public abstract void UpdateBuffer();
    public abstract ShaderProgram CreateShaderProgram(params ShaderDescription[] descriptions);
    public abstract Pipeline CreatePipeline(PipelineDescription description);
    public abstract CommandList CreateCommandList();
    public abstract void SubmitCommands();
    public abstract void SwapBuffers();
    public abstract void WaitIdle();
    public abstract void Dispose();
    
    // TODO: This was for testing and should be removed
    public abstract void Render();
}

public struct GraphicsDeviceDescription
{
    
}

public enum GraphicsBackend
{
    Vulkan,
}