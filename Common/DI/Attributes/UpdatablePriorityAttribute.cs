namespace Common.DI.Attributes;

/// <summary>
///     Sets the priority of a tickable service.
/// </summary>
/// <remarks>The higher priority values get ticked first.</remarks>>
[AttributeUsage(AttributeTargets.Class)]
public class UpdatablePriorityAttribute : Attribute
{
    /// <summary>
    ///     The tickable priority.
    /// </summary>
    public EPriority Priority { get; }

    /// <summary>
    ///     Defines the priority to use for ticking.
    /// </summary>
    /// <param name="priority"></param>
    public UpdatablePriorityAttribute(EPriority priority = EPriority.Normal)
    {
        Priority = priority;
    }
}
