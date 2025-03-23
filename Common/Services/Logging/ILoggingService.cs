using Common.DI;

namespace Common.Logging;

/// <summary>
///     A service contract for logging events or messages.
/// </summary>
public interface ILoggingService : IService
{
    
    /// <summary>
    ///     Log using debug output.
    /// </summary>
    /// <param name="message"></param>
    void Debug(string message);
    
    /// <summary>
    ///     Log using information output.
    /// </summary>
    /// <param name="message"></param>
    void Information(string message);
    
    /// <summary>
    ///     Log using warning output.
    /// </summary>
    /// <param name="message"></param>
    void Warning(string message);
    
    /// <summary>
    ///     Log using error output.
    /// </summary>
    /// <param name="message"></param>
    void Error(string message);
    
    /// <summary>
    ///     Log using fatal output.
    /// </summary>
    /// <param name="message"></param>
    void Fatal(string message);
}