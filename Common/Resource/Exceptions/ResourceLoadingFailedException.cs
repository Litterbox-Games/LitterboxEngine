namespace Common.Resource.Exceptions;

/// <summary>
///     A resource file was found, but failed to load.
/// </summary>
public class ResourceLoadingFailedException : Exception
{
    /// <inheritdoc />
    public ResourceLoadingFailedException(string message) : base(message) { }
}