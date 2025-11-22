namespace Common.Core;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GameAttribute(EMode modes = EMode.Both) : Attribute;


[AttributeUsage(AttributeTargets.Class)]
public sealed class EngineAttribute : Attribute;


[AttributeUsage(AttributeTargets.Class)]
public sealed class MultiplayerAttribute : Attribute;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AsAttribute<T>(string? key = null) : Attribute where T : IService;

[AttributeUsage(AttributeTargets.Method)]
public sealed class PriorityAttribute(float priority) : Attribute
{
    public float Priority { get; } = priority;
    public PriorityAttribute(EPriority priority) : this((float)priority) { }
}