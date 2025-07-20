using Client.Graphics.Backend;
using Common.Services.Resource;

namespace Client.Services.Resource;

public interface IGraphicsResource
{
    public IResource UploadToGraphicsDevice(IGraphicsDeviceService graphicsDevice);
}