namespace Client.Graphics;

public interface IGuiDrawable
{
    /// <summary>
    ///     Called every game GUI draw tick.
    /// </summary>
    void DrawGui(float deltaTime, RendererService renderer);
}