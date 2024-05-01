namespace Common.DI;

/// <summary>
///     A service capable of executing game or draw ticks.
/// </summary>
public interface ITickableService : IService
{
    /// <summary>
    ///     Called every game update tick.
    /// </summary>
    /// <param name="deltaTime">The amount of time in seconds since the last tick.</param>
    void Update(float deltaTime);
    
    /// <summary>
    ///     Called every game draw tick.
    /// </summary>
    void Draw();
}