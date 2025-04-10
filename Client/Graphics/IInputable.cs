namespace Client.Graphics;

public interface IInputable
{
    /// <summary>
    ///     Called every game input tick.
    /// </summary>
    public void Input(InputService input);
}