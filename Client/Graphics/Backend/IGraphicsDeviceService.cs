using Client.Services.Resource;
using Common.Core;

namespace Client.Graphics.Backend;

public interface IGraphicsDeviceService: IService
{
    public CommandList CommandList { get; }
    public Buffer CreateBuffer(BufferDescription description);
    public void UpdateBuffer(Buffer buffer, uint offset, uint[] data);
    public ShaderProgram CreateShaderProgram(params ShaderDescription[] descriptions);
    public Texture CreateTexture(uint width, uint height, Span<byte> data);
    public Texture CreateTexture(uint width, uint height, RgbaByte color);
    public Pipeline CreatePipeline(PipelineDescription description);
    public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description);
    public ResourceSet CreateResourceSet(ResourceLayout layout);
    public Sampler CreateSampler();
    public void WaitIdle();
    public void BeginFrame();
    public void EndFrame();
}