namespace Common.Core.Attributes;

[Flags]
public enum EMode
{
    Host   = 0x10,
    Client = 0x01,
    Both   = Host | Client
}


[AttributeUsage(AttributeTargets.Class)]
public sealed class GameAttribute(EMode modes = EMode.Both) : Attribute;


[AttributeUsage(AttributeTargets.Class)]
public sealed class EngineAttribute : Attribute;


[AttributeUsage(AttributeTargets.Class)]
public sealed class MultiplayerAttribute : Attribute;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AsAttribute<T>(string? key = null) : Attribute where T : IService;
