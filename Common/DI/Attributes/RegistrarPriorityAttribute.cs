namespace Common.DI.Attributes;

/// <summary>
///     Sets the registration priority of a registrar.
/// </summary>
/// <remarks>The higher priority values get registered FIRST.</remarks>>
[AttributeUsage(AttributeTargets.Class)]
public class RegistrarPriorityAttribute : Attribute
{
    /// <summary>
    ///     The registration priority.
    /// </summary>
    public EPriority Priority { get; }

    /// <summary>
    ///     Defines the priority to use for registration.
    /// </summary>
    /// <param name="priority"></param>
    public RegistrarPriorityAttribute(EPriority priority = EPriority.Normal)
    {
        Priority = priority;
    }
}