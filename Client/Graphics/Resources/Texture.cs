using Client.Graphics.GHAL;
using SixLabors.ImageSharp.PixelFormats;

namespace Client.Graphics.Resources;

public abstract class Texture : IResource, IDisposable
{
    public readonly  uint Width;
    public readonly uint Height;

    internal Texture(uint width, uint height)
    {
        Width = width;
        Height = height;
    }
    
    public static IResource LoadFromFile(string path, GraphicsDevice? graphicsDevice)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

        if (graphicsDevice == null)
            throw new Exception("A graphics device is required to load textures");
        
        var sizeInBytes = image.Width * image.Height * image.PixelType.BitsPerPixel / 8;
        
        var data = new byte[sizeInBytes];
        image.CopyPixelDataTo(data);

        return graphicsDevice.CreateTexture((uint)image.Width, (uint)image.Height, data);
    }

    public abstract void Dispose();
}