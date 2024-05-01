using System;
using Serilog;
using Serilog.Core;

namespace Common.Logging;

/// <summary>
///     A service implementing the logging contract to log information to the system console.
/// </summary>
public class ConsoleLoggingService : ILoggingService, IDisposable
{
    private readonly Logger _logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

    /// <inheritdoc />
    public void Debug(string message)
    {
        _logger.Debug(message);
    }

    /// <inheritdoc />
    public void Information(string message)
    {
        _logger.Information(message);
    }

    /// <inheritdoc />
    public void Warning(string message)
    {
        _logger.Warning(message);
    }

    /// <inheritdoc />
    public void Error(string message)
    {
        _logger.Error(message);
    }

    /// <inheritdoc />
    public void Fatal(string message)
    {
        _logger.Fatal(message);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _logger.Dispose();
        
        GC.SuppressFinalize(this);
    }
}