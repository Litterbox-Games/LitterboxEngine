using Client.Graphics;
using Common.Host;

namespace Client.Host;

public interface IClientHost: IHost
{
    internal List<IDrawable> GameDrawables { get; }
    internal List<IGuiDrawable> GameGuiDrawables { get; }
    internal List<IInputable> GameInputables { get; }
    
    public void Input(InputService input)
    {
        GameInputables.ForEach(inputable => inputable.Input(input));
    }
    
    public void Draw(float deltaTime, RendererService renderer)
    {
        GameDrawables.ForEach(drawable => drawable.Draw(deltaTime, renderer));
    }
    
    public void DrawGui(float deltaTime, RendererService renderer)
    {
        GameGuiDrawables.ForEach(drawable => drawable.DrawGui(deltaTime, renderer));
    }
}