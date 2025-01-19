using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.IO;
using Client.Graphics.GHAL;
using Common.Resource;

namespace Client.Resource;

public class Aseprite : IResource, IGraphicsResource, IReloadable, IDisposable
{
    public readonly Texture Texture;
    
    private Aseprite(Texture texture)
    {
        Texture = texture;
    }
    
    public IResource UploadToGraphicsDevice(IGraphicsDeviceService graphicsDeviceService)
    {
        var texture = graphicsDeviceService.CreateTexture(Texture.Width, Texture.Height, Texture.Data);
        return new Aseprite(texture);
    }

    public IResource Reload(string path)
    {
        var file = AsepriteFileLoader.FromFile(path);
        
        var size = file.Frames[0].Size;
        var frame = file.Frames[0].FlattenFrame(onlyVisibleLayers: true, includeBackgroundLayer: false, includeTilemapCels: false);

        var data = new byte[size.Width * size.Height * 4];
        var index = 0;
        foreach (var pixel in frame)
        {
            // Assuming 'pixel' is a struct with R, G, B, A properties (each byte representing the color channels)
            data[index++] = pixel.R; // Red
            data[index++] = pixel.G; // Green
            data[index++] = pixel.B; // Blue
            data[index++] = pixel.A; // Alpha
        }
        
        var texture = Texture.FromData((uint)size.Width, (uint)size.Height, data);
        
        return new Aseprite(texture);
    }
    
    public static IResource LoadFromFile(string path)
    {
        var file = AsepriteFileLoader.FromFile(path);
        
        var size = file.Frames[0].Size;
        var frame = file.Frames[0].FlattenFrame(onlyVisibleLayers: true, includeBackgroundLayer: false, includeTilemapCels: false);

        var data = new byte[size.Width * size.Height * 4];
        var index = 0;
        foreach (var pixel in frame)
        {
            // Assuming 'pixel' is a struct with R, G, B, A properties (each byte representing the color channels)
            data[index++] = pixel.R; // Red
            data[index++] = pixel.G; // Green
            data[index++] = pixel.B; // Blue
            data[index++] = pixel.A; // Alpha
        }
        
        var texture = Texture.FromData((uint)size.Width, (uint)size.Height, data);
        
        return new Aseprite(texture);
    }

    public void Dispose()
    {
        Texture.Dispose();
        GC.SuppressFinalize(this);
    }
}