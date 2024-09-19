using Client.Resource;

namespace Client.Graphics.GHAL;

public abstract class ResourceSet
{
    // TODO: we should overload this to only accept storage buffers in the future?
    public abstract void UpdateStorageBuffer(uint binding, Buffer buffer, uint index = 0);
    public abstract void Update(uint binding, Buffer buffer, uint index = 0);
    public abstract void Update(uint binding, Texture texture, uint index = 0);
    public abstract void Update(uint binding, Sampler sampler, uint index = 0);
}