using System.Collections.Generic;
using System.Linq;
using Common.DI;

namespace Common.Logging;

/// <summary>
///     The root logging service. Registered under the default mapping, this sends log events to every logging service registered.
/// </summary>
public class RootLoggingService : ILoggingService
{
    private readonly List<ILoggingService> _loggers = new();
    private readonly IContainer _container;
    
    /// <summary>
    ///     Creates an instance of the root logger.
    /// </summary>
    /// <remarks>This is designed to be called by the container during resolution, not by manual invocation.</remarks>
    public RootLoggingService(IContainer container)
    {
        _container = container;
        RefreshLoggers();
    }
    
    /// <inheritdoc />
    public void Debug(string message)
    {
        _loggers.ForEach(x => x.Debug(message));
    }

    /// <inheritdoc />
    public void Information(string message)
    {
        _loggers.ForEach(x => x.Information(message));
    }

    /// <inheritdoc />
    public void Warning(string message)
    {
        _loggers.ForEach(x => x.Warning(message));
    }

    /// <inheritdoc />
    public void Error(string message)
    {
        _loggers.ForEach(x => x.Error(message));
    }

    /// <inheritdoc />
    public void Fatal(string message)
    {
        _loggers.ForEach(x => x.Fatal(message));
    }

    /// <summary>
    ///     Forces the root logger to clear and resolve all other logging services again.
    /// </summary>
    /// <remarks>This is used when a logging service is added after initial registration.</remarks>
    public void RefreshLoggers()
    {
        _loggers.Clear();
        _loggers.AddRange(_container.ResolveAll<ILoggingService>().Where(x => x != this));
    }
}