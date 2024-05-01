﻿using Client.Graphics.Resources;

namespace Client.Graphics.GHAL;

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

    public abstract Buffer CreateBuffer(BufferDescription description);
    public abstract void UpdateBuffer(Buffer buffer, uint offset, uint[] data);
    public abstract ShaderProgram CreateShaderProgram(params ShaderDescription[] descriptions);
    public abstract Texture CreateTexture(uint width, uint height, Span<byte> data);
    public abstract Texture CreateTexture(uint width, uint height, RgbaByte color);
    public abstract Pipeline CreatePipeline(PipelineDescription description);
    public abstract ResourceLayout CreateResourceLayout(ResourceLayoutDescription description);
    public abstract ResourceSet CreateResourceSet(ResourceLayout layout);
    public abstract Sampler CreateSampler();
    public abstract CommandList CreateCommandList();
    public abstract void SubmitCommands();
    public abstract void SwapBuffers();
    public abstract void WaitIdle();
    public abstract void Dispose();
}

public struct GraphicsDeviceDescription
{
    
}

public enum GraphicsBackend
{
    Vulkan,
}