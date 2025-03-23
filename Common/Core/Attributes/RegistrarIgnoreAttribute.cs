namespace Common.DI.Attributes;

/// <summary>
///     Do not register this service during the primary registration.
/// </summary>
/// <remarks>Any registrar that implements this attribute must have their registration function manually.</remarks>>
[AttributeUsage(AttributeTargets.Class)]
public class RegistrarIgnoreAttribute : Attribute { }