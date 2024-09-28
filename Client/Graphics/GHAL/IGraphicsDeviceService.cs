using Client.Graphics.Input;
using Client.Graphics.Input.ImGui;
using Client.Resource;
using Common.DI;

namespace Client.Graphics.GHAL;

public interface IGraphicsDeviceService: IService
{
    public ImGui InitImGui();
    public Buffer CreateBuffer(BufferDescription description);
    public void UpdateBuffer(Buffer buffer, uint offset, uint[] data);
    public ShaderProgram CreateShaderProgram(params ShaderDescription[] descriptions);
    public Texture CreateTexture(uint width, uint height, Span<byte> data);
    public Texture CreateTexture(uint width, uint height, RgbaByte color);
    public Pipeline CreatePipeline(PipelineDescription description);
    public ResourceLayout CreateResourceLayout(ResourceLayoutDescription description);
    public ResourceSet CreateResourceSet(ResourceLayout layout);
    public Sampler CreateSampler();
    public CommandList CreateCommandList();
    public void SubmitCommands();
    public void SwapBuffers();
    public void WaitIdle();
}