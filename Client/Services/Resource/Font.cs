using Client.Graphics.GHAL;
using Common.Mathematics;
using Common.Services.Resource;
using MoreLinq.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Rectangle = System.Drawing.Rectangle;

namespace Client.Services.Resource;

public struct Glyph
{
    public char Character;
    public Rectangle Source;
}

public class Font(Dictionary<char, Glyph> glyphs, Texture texture) : IResource, IGraphicsResource
{
    public readonly Dictionary<char , Glyph> Glyphs = glyphs;
    public readonly Texture Texture = texture;

    public IResource UploadToGraphicsDevice(IGraphicsDevice graphicsDevice)
    {
        // So fucking hacky, we gotta change how the client loads resources
        var texture = graphicsDevice.CreateTexture(Texture.Width, Texture.Width, Texture.Data);
        return new Font(Glyphs, texture);
    }
    
    public static IResource LoadFromFile(string path)
    {
        // TODO: make these available through a JSON file?
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()_+{}[]<>?:/\\~ ";
        const int fontSize = 8;
        
        var font = new FontCollection().Add(path).CreateFont(fontSize);

        var textOptions = new RichTextOptions(font)
        {
            KerningMode = KerningMode.None,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        
        // Create glyphs and calculate texture atlas size
        var glyphs = new Dictionary<char, Glyph>();
        var size = chars.Aggregate(Vector2i.Zero, (cursor, c) =>
        {
            var bounds = TextMeasurer.MeasureBounds(c.ToString(), textOptions);
            var glyphWidth = (int)Math.Ceiling(bounds.Width + bounds.X);
            var glyphHeight = (int)Math.Ceiling(bounds.Height + bounds.Y);
            
            glyphs[c] = new Glyph
            {
                Character = c,
                Source = new Rectangle(cursor.X, 0, glyphWidth, glyphHeight),
            };

            return new Vector2i(cursor.X + glyphWidth, Math.Max(cursor.Y, glyphHeight));
        });
        
        var image = new Image<Rgba32>(size.X, size.Y);
        
        // Draw texture atlas
        var graphicsOptions = new GraphicsOptions { Antialias = false };
        var drawingOptions = new DrawingOptions { GraphicsOptions = graphicsOptions };
        
        glyphs.Values.ForEach(glyph => image.Mutate(ctx =>
            {
                var cursor = new Point(glyph.Source.X, glyph.Source.Y);
                ctx.DrawText(drawingOptions, glyph.Character.ToString(), font, Color.White, cursor);
            })
        );
        
        // Copy image data to texture resource
        var sizeInBytes = image.Width * image.Height * image.PixelType.BitsPerPixel / 8;
        var data = new byte[sizeInBytes];
        image.CopyPixelDataTo(data);
        
        var texture = Texture.FromData((uint) image.Width, (uint) image.Height, data);
        
        return new Font(glyphs, texture);
    }
}