// See https://aka.ms/new-console-template for more information

using LitterboxEngine;
using Silk.NET.GLFW;

namespace SampleGame;

public class Program: IGame
{
    public static void Main()
    {
        var engine = new Engine("Sample Game", new Program());
        engine.Start();
    }
    
    public void Init(Window window)
    {
        // throw new NotImplementedException();
    }

    public void Input(Window window)
    {
        if (window.IsKeyPressed(Keys.A))
            Console.WriteLine("A");
        
        // throw new NotImplementedException();
    }

    public void Update(Window window)
    {
        // throw new NotImplementedException();
    }
    
    public void Dispose()
    {
        // throw new NotImplementedException();
    }
}