using LitterboxEngine.Graphics;
using Silk.NET.GLFW;

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
        renderer.Begin();
        renderer.End();
    }

    public void Dispose()
    {
        
    }
}