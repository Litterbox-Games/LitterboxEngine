namespace Common.DI;

public interface IUpdatable
{
    /// <summary>
    ///     Called every game update tick.
    /// </summary>
    /// <param name="deltaTime">The amount of time in seconds since the last tick.</param>
    void Update(float deltaTime);
}