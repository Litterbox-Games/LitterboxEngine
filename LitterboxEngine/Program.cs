using System.Drawing;
using LitterboxEngine.Graphics;
using Silk.NET.GLFW;
using Color = LitterboxEngine.Graphics.Color;

namespace LitterboxEngine;

public class Program: IGame
{
    public static void Main()
    {
        var engine = new Engine("Sample Game", new Program());
        engine.Start();
        engine.Dispose();
    }
    
    public void Init(Window window)
    {
        
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
        // TODO: should the engine called begin and end for you? We would need to change how clear color works most likely or the first frame will have the default color
        renderer.Begin();
        
        renderer.DrawRectangle(new RectangleF(0, 0, 100, 100), new Color(1, 0, 0, 0));
        
        renderer.DrawRectangle(new RectangleF(0.5f, 0.5f, 0.5f, 0.5f), new Color(1, 0, 0, 0));
        
        renderer.End();
    }

    public void Dispose()
    {
        
    }
}