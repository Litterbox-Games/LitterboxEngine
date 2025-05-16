namespace Client.Graphics;

public interface IDrawable
{
    /// <summary>
    ///     Called every game draw tick.
    /// </summary>
    void Draw(float deltaTime, RendererService renderer);
}