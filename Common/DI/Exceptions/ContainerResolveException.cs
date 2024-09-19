namespace Common.DI.Exceptions;

/// <summary>
///     This exception is thrown when a service is not resolved.
/// </summary>
/// <remarks>This exception should be considered fatal.</remarks>>
public class ContainerResolveException : Exception
{
    /// <summary>
    ///     Initialize the exception.
    /// </summary>
    /// <param name="contract">The type associated with the contract that failed to resolve.</param>
    /// <param name="mapping">The mapping for the type that the container tried to resolve. Default registrations should pass null.</param>
    public ContainerResolveException(Type contract, string? mapping = null) : 
        base(
            mapping == null ? 
                $"A resolve for {contract.FullName} has failed." : 
                $"A resolve for {contract.FullName} under the mapping {mapping} has failed."
            ) { }
}