using Common.Host;

namespace Common.DI.Attributes;

/// <summary>
///     Define the game states in which this registration is called.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegistrarModeAttribute : Attribute
{
    /// <summary>
    ///     The flags to define game states in which the service is registered.
    /// </summary>
    public EGameMode GameMode { get; }

    /// <summary>
    ///     Initialize the attribute with the given mode flags.
    /// </summary>
    /// <param name="mode">The flags under which this service is registered.</param>
    public RegistrarModeAttribute(EGameMode mode)
    {
        GameMode = mode;
    }
}