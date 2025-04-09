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
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()_+{}[]<>?:/\\~";
        const int fontSize = 8;
        
        var font = new FontCollection().Add(path).CreateFont(fontSize);

        var textOptions = new RichTextOptions(font)
        {
            KerningMode = KerningMode.None,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        
        var glyphs = new Dictionary<char, Glyph>();
        var size = chars.Aggregate(Vector2i.Zero, (size, c) =>
        {
            var charSize = TextMeasurer.MeasureAdvance(c.ToString(), textOptions);
            var glyphWidth = (int)Math.Ceiling(charSize.Width);
            var glyphHeight = (int)Math.Ceiling(charSize.Height + charSize.X);
            
            glyphs[c] = new Glyph
            {
                Character = c,
                Source = new Rectangle(size.X, 0, glyphWidth, glyphHeight),
            };

            return new Vector2i(size.X + glyphWidth, Math.Max(size.Y, glyphHeight));
        });
        
        var image = new Image<Rgba32>(size.X, 8);
        
        var graphicsOptions = new GraphicsOptions { Antialias = false };
        var drawingOptions = new DrawingOptions { GraphicsOptions = graphicsOptions };
        glyphs.Values.ForEach(glyph => image.Mutate(ctx =>
            {
                // var cursor = new Point(glyph.Source.X, glyph.Source.Y);
                var cursor = new Point(glyph.Source.X, glyph.Source.Y);
                ctx.DrawText(drawingOptions, glyph.Character.ToString(), font, Color.White, cursor);
                // ctx.DrawText(drawingOptions, textOptions, "q", new SolidBrush(Color.White), null);
                // ctx.DrawText(drawingOptions, textOptions, chars, new SolidBrush(Color.White), null);
            })
        );
        
        var sizeInBytes = image.Width * image.Height * image.PixelType.BitsPerPixel / 8;
        
        image.Save("dogica.png");
        
        var data = new byte[sizeInBytes];
        image.CopyPixelDataTo(data);
        
        var texture = Texture.FromData((uint) image.Width, (uint) image.Height, data);
        
        return new Font(glyphs, texture);
    }
}