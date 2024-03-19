using LitterboxEngine.Graphics;

namespace LitterboxEngine;

public interface IGame: IDisposable
{
    public void Init(Window window);

    public void Input(Window window);
    
    public void Update(Window window);

    public void Draw(Renderer renderer);
}