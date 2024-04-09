using System.Drawing;
using LitterboxEngine.Graphics;
using LitterboxEngine.Graphics.Resources;
using LitterboxEngine.Resource;
using Silk.NET.GLFW;

namespace LitterboxEngine;

public class Program: IGame
{
    private Texture _logo = null!;
    private Texture _objects = null!;
    
    public static void Main()
    {
        var engine = new Engine("Sample Game", new Program());
        engine.Start();
        engine.Dispose();
    }
    
    public void Init(Window window)
    {
        _logo = ResourceManager.Get<Texture>("Resources/Textures/litterbox_logo.png");
        _objects = ResourceManager.Get<Texture>("Resources/Textures/Objects.png");
    }

    public void Input(Window window)
    {
        if (window.IsKeyPressed(Keys.A))
            Console.WriteLine("A");
    }

    public void Update(Window window)
    {
        
    }

    public void Draw(Renderer renderer)
    {
        renderer.Begin();
        
        renderer.DrawRectangle(new RectangleF(0, 0, 100f, 100f), Color.Yellow);
        renderer.DrawRectangle(new RectangleF(100, 0, 100f, 100f), Color.Red);
        renderer.DrawRectangle(new RectangleF(0, 100, 100f, 100f), Color.Blue);
        
        renderer.DrawTexture(_logo, new RectangleF(100, 100, 100f, 100f), Color.Purple);
        renderer.DrawTexture(_objects, new Rectangle(16, 16, 16, 16), new RectangleF(200, 0, 100f, 100f), Color.White);
        
        renderer.End();
    }

    public void Dispose()
    {
        
    }
}