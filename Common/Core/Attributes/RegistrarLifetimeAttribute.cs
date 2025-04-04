namespace Common.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RegistrarLifetimeAttribute(ELifetime lifetime): Attribute
{
    public ELifetime Lifetime { get; } = lifetime;
}