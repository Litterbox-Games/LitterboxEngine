using Common.Host;
using Unity;

namespace Common.DI;

/// <summary>
///     The contract representing the host container of an application/game state.
/// </summary>
public interface IContainer : IService, IDisposable
{
    /// <summary>
    ///     The current game mode of the host.
    /// </summary>
    EGameMode GameMode { get; }

    /// <summary>
    ///     Creates a singleton registration in the container.
    /// </summary>
    /// <remarks>
    ///     Singleton will be created upon first resolution.
    /// </remarks>
    /// <param name="mapping">The string mapping to be used.</param>
    void RegisterSingleton<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService;
    
    /// <summary>
    ///     Creates a transient registration in the container.
    /// </summary>
    /// <remarks>
    ///     Upon every resolution, a new instance of the service will be created each time.
    /// </remarks>
    /// <param name="mapping">The string mapping to be used.</param>
    void RegisterTransient<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService;
    
    /// <summary>
    ///     Creates a threaded registration in the container.
    /// </summary>
    /// <remarks>
    ///     Acts as a singleton where each thread is assigned its own instance upon resolving.
    /// </remarks>
    /// <param name="mapping">The string mapping to be used.</param>
    void RegisterThreadedSingleton<TContract, TInstance>(string? mapping = null) where TInstance : TContract where TContract : IService;

    /// <summary>
    ///     Creates a singleton registration in the container using the provided instance.
    /// </summary>
    /// <remarks>
    ///     Singleton is created before the registration.
    /// </remarks>
    /// <param name="instance">The instance of the object to use as the singleton.</param>
    /// <param name="performBuildup">Whether the container should attempt property injection on the passed instance.</param>
    /// <param name="mapping">The string mapping to be used.</param>
    void RegisterSingleton<TContract, TInstance>(TInstance instance, bool performBuildup, string? mapping = null) where TInstance : TContract where TContract : IService;

    /// <summary>
    ///     Resolves a service using the given contract.
    /// </summary>
    /// <param name="mapping">The mapping used to resolve the service.</param>
    /// <returns>An instance of the resolved contract.</returns>
    T Resolve<T>(string? mapping = null) where T : IService;
    
    /// <summary>
    ///     Resolves all services using the given contract.
    /// </summary>
    /// <returns>All services implementing the given contract.</returns>
    /// <remarks>This does not resolve the default service, only mapped services.</remarks>>
    IEnumerable<T> ResolveAll<T>() where T : IService;
}