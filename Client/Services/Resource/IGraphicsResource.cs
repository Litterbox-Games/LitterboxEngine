using Client.Graphics.GHAL;
using Common.Services.Resource;

namespace Client.Services.Resource;

public interface IGraphicsResource
{
    public IResource UploadToGraphicsDevice(IGraphicsDevice graphicsDevice);
}