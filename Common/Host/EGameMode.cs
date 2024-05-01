namespace Common.Host;

/// <summary>
///     Flags representing a game mode or state.
/// </summary>
[Flags]
public enum EGameMode
{
    /// <summary>
    ///     Single player instance, similar to host, but without the networking services.
    /// </summary>
    SinglePlayer = 0x0001,
    
    /// <summary>
    ///     Client instance, for a client connected to a different host.
    /// </summary>
    Client = 0x0010,
    
    /// <summary>
    ///     Server instance, for servers hosted through the game application with a local client.
    /// </summary>
    Host = 0x0100,
    
    /// <summary>
    ///     Server instance, for dedicated servers without a local client.
    /// </summary>
    Dedicated = 0x1000
}