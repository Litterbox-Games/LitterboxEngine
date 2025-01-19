using System.Drawing;
using Client.Graphics.GHAL;
using Common.Resource;
using SixLabors.ImageSharp.PixelFormats;

namespace Client.Resource;

public class Texture : IResource, IGraphicsResource, IReloadable, IDisposable
{
    public readonly uint Width;
    public readonly uint Height;
    public readonly byte[] Data;

    private protected Texture(uint width, uint height, byte[] data)
    {
        Width = width;
        Height = height;
        Data = data;
    }
    
    public Rectangle GetSourceRectangle(int x, int y)
    {
        var xTexCoord = 16 * x;
        var yTexCoord = 16 * y;

        return new Rectangle(xTexCoord, yTexCoord, 16, 16);
    }

    public IResource UploadToGraphicsDevice(IGraphicsDeviceService graphicsDeviceService)
    {
        return graphicsDeviceService.CreateTexture(Width, Height, Data);
    }
    
    public static IResource LoadFromFile(string path)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

        var sizeInBytes = image.Width * image.Height * image.PixelType.BitsPerPixel / 8;
        
        var data = new byte[sizeInBytes];
        image.CopyPixelDataTo(data);

        return new Texture((uint) image.Width, (uint) image.Height, data);
    }

    public static Texture FromData(uint width, uint height, byte[] data)
    {
        return new Texture(width, height, data);
    }

    public IResource Reload(string path)
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