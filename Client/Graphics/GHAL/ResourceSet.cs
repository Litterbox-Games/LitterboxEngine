using Client.Resource;

namespace Client.Graphics.GHAL;

public abstract class ResourceSet
{
    public abstract void Update(uint binding, Buffer buffer, uint index = 0);
    public abstract void Update(uint binding, Texture texture, uint index = 0);
    public abstract void Update(uint binding, Sampler sampler, uint index = 0);
}