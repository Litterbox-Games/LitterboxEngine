using System.Drawing;
using Client.Graphics.GHAL;
using Common.Resource;
using SixLabors.ImageSharp.PixelFormats;

namespace Client.Resource;

public class Texture : IResource, IGraphicsResource, IDisposable
{
    public readonly uint Width;
    public readonly uint Height;
    private readonly byte[] _data;

    private protected Texture(uint width, uint height, byte[] data)
    {
        Width = width;
        Height = height;
        _data = data;
    }
    
    public Rectangle GetSourceRectangle(int x, int y)
    {
        var xTexCoord = 16 * x;
        var yTexCoord = 16 * y;

        return new Rectangle(xTexCoord, yTexCoord, 16, 16);
    }

    public IResource UploadToGraphicsDevice(IGraphicsDeviceService graphicsDeviceService)
    {
        return graphicsDeviceService.CreateTexture(Width, Height, _data);
    }
    
    public static IResource LoadFromFile(string path)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

        var sizeInBytes = image.Width * image.Height * image.PixelType.BitsPerPixel / 8;
        
        var data = new byte[sizeInBytes];
        image.CopyPixelDataTo(data);

        return new Texture((uint) image.Width, (uint) image.Height, data);
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}