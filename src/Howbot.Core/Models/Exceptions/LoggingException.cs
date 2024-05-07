namespace Howbot.Core.Models.Exceptions;

/// <summary>
///   Created this exception to use try/catch in logger adapter.
///   These exceptions, when bubbled up should not cause bot to stop
/// </summary>
public class LoggingException(string message) : Exception(message);
