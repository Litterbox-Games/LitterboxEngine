namespace Common.DI;

/// <summary>
///     A simple priority enum.
/// </summary>
public enum EPriority
{
    /// <summary>
    ///     The lowest priority.
    /// </summary>
    VeryLow = -2,
    
    /// <summary>
    ///     A middle ground between normal and very low.
    /// </summary>
    Low = -1,
    
    /// <summary>
    ///     Normal priority.
    /// </summary>
    /// <remarks>This is the default for anything that does not specify a priority.</remarks>
    Normal = 0,
    
    /// <summary>
    ///     High priority.
    /// </summary>
    /// <remarks>This is the priority used by most engine/host services.</remarks>
    High = 1,
    
    /// <summary>
    ///     The highest priority available.
    /// </summary>
    VeryHigh = 2
}