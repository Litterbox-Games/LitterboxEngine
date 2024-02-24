namespace LitterboxEngine.Graphics;

public abstract class GraphicsDevice
{
    public static GraphicsDevice Create(GraphicsBackend backend)
    {
        return backend switch
        {
            GraphicsBackend.Vulkan => CreateVulkanGraphicsDevice(),
            _ => throw new NotImplementedException($"A GraphicsDevice for {backend} has not been implemented")
        };
    }
    
    private static GraphicsDevice CreateVulkanGraphicsDevice()
    {
        throw new NotImplementedException();
    }

    public abstract void CreateBuffer();
    public abstract void UpdateBuffer();
    public abstract void CreateShader();
    public abstract void CreatePipeline();
    public abstract void CreatCommandList();
}

public enum GraphicsBackend
{
    Vulkan,
}