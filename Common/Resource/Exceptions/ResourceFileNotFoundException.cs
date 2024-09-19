namespace Common.Resource.Exceptions;

/// <summary>
///     A resource file was not found.
/// </summary>
public class ResourceFileNotFoundException : FileNotFoundException
{
    /// <inheritdoc />
    public ResourceFileNotFoundException(string message) : base(message) { }
}