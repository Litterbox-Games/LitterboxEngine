using System.Drawing;
using System.Numerics;
using Client.Graphics.Resources;
using Common.DI;

namespace Client.Graphics;

public interface IRendererService: IService, IDisposable
{
    public Color ClearColor { get; set; }
    
    public void SetViewSize(Vector2 size);

    public void Begin(Matrix4x4? view = null);

    public void End();

    public void DrawRectangle(RectangleF destination, Color color, float depth = 0.0f);

    public void DrawTexture(Texture texture, RectangleF destination, Color color, float depth = 0.0f);

    public void DrawTexture(Texture texture, Rectangle source, RectangleF destination, Color color, float depth = 1.0f);
}