using Client.Graphics.GHAL;
using Common.Resource;

namespace Client.Resource;

public interface IGraphicsResource
{
    public IResource UploadToGraphicsDevice(IGraphicsDevice graphicsDevice);
}