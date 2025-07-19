using Ardalis.GuardClauses;
using Howbot.Infrastructure.Exceptions;
using Howbot.SharedKernel;
using Microsoft.Extensions.Logging;

namespace Howbot.Infrastructure;

/// <summary>
///   An ILoggerAdapter implementation that uses Microsoft.Extensions.Logging
/// </summary>
/// <typeparam name="T"></typeparam>
public class LoggerAdapter<T>(ILogger<LoggerAdapter<T>> logger) : ILoggerAdapter<T>
{
  public ILoggerAdapter<T> CastToLoggerClass()
  {
    return this as ILoggerAdapter<T>;
  }

  /// <summary>
  ///   Logs a message with the specified severity.
  /// </summary>
  /// <param name="severity">The severity level of the log message.</param>
  /// <param name="message">The log message.</param>
  /// <param name="args">Optional arguments to format the log message.</param>
  /// <exception cref="LoggingException">Thrown when an exception occurs while logging.</exception>
  public void Log(LogLevel severity, string message, params object[] args)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(message, nameof(message));

      logger.Log(severity, message, args);
    }
    catch (Exception e)
    {
      logger.LogError(e, nameof(Log));
      throw new LoggingException(e.Message);
    }
  }

  /// <summary>
  ///   Logs an error message along with the specified exception.
  /// </summary>
  /// <param name="exception">The exception to be logged.</param>
  /// <exception cref="LoggingException">Thrown if an error occurs while logging the exception.</exception>
  public void LogError(Exception exception)
  {
    try
    {
      Guard.Against.Null(exception, nameof(exception));

      logger.LogError(exception, "An exception has been thrown");
    }
    catch (Exception e)
    {
      logger.LogError(e, nameof(LogError));
      throw new LoggingException(e.Message);
    }
  }

  /// <summary>
  ///   Logs an error message.
  /// </summary>
  /// <param name="message">The error message.</param>
  /// <param name="args">Optional arguments to format the error message.</param>
  /// <exception cref="LoggingException">Thrown when an error occurs while logging the error message.</exception>
  public void LogError(string message, params object[] args)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(message, nameof(message));

      logger.LogError(message, args);
    }
    catch (Exception e)
    {
      logger.LogError(e, nameof(LogError));
      throw new LoggingException(e.Message);
    }
  }

  /// <summary>
  ///   Logs an error message along with an exception.
  /// </summary>
  /// <param name="exception">The exception to be logged.</param>
  /// <param name="message">The error message to be logged.</param>
  /// <param name="args">Optional arguments to format the error message.</param>
  /// <exception cref="LoggingException">Thrown when an error occurs while logging.</exception>
  public void LogError(Exception exception, string message, params object[] args)
  {
    try
    {
      Guard.Against.Null(exception, nameof(exception));
      Guard.Against.NullOrWhiteSpace(message, nameof(message));

      logger.LogError(exception, message, args);
    }
    catch (Exception e)
    {
      logger.LogError(e, nameof(LogError));
      throw new LoggingException(e.Message);
    }
  }

  /// <summary>
  ///   Logs information message.
  /// </summary>
  /// <param name="message">The message to log.</param>
  /// <param name="args">The optional format parameters.</param>
  /// <exception cref="LoggingException">Thrown when an error occurs during logging.</exception>
  public void LogInformation(string message, params object[] args)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(message, nameof(message));

      logger.LogInformation(message, args);
    }
    catch (Exception e)
    {
      logger.LogError(e, nameof(LogInformation));
      throw new LoggingException(e.Message);
    }
  }

  /// <summary>
  ///   Logs a debug message with optional arguments.
  /// </summary>
  /// <param name="message">The debug message to be logged.</param>
  /// <param name="args">Optional arguments to format the debug message.</param>
  /// <exception cref="LoggingException">Thrown when an error occurs during logging.</exception>
  public void LogDebug(string message, params object[] args)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(message, nameof(message));

      logger.LogDebug(message, args);
    }
    catch (Exception e)
    {
      logger.LogError(e, nameof(LogDebug));
      throw new LoggingException(e.Message);
    }
  }

  /// <summary>
  ///   Logs a warning message.
  /// </summary>
  /// <param name="message">The warning message to be logged.</param>
  /// <param name="args">Optional parameters to be formatted into the warning message.</param>
  /// <exception cref="LoggingException">Thrown when an error occurs while logging the warning message.</exception>
  public void LogWarning(string message, params object[] args)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(message, nameof(message));

      logger.LogWarning(message, args);
    }
    catch (Exception e)
    {
      logger.LogError(e, nameof(LogWarning));
      throw new LoggingException(e.Message);
    }
  }

  /// <summary>
  ///   Logs a critical exception with a message and optional arguments.
  ///   Throws a LoggingException if an error occurs during logging.
  /// </summary>
  /// <param name="exception">The exception to log.</param>
  /// <param name="message">The message to log.</param>
  /// <param name="args">Optional arguments to format into the message.</param>
  /// <exception cref="LoggingException">Thrown if an error occurs during logging.</exception>
  public void LogCritical(Exception exception, string message, params object[] args)
  {
    try
    {
      Guard.Against.Null(exception, nameof(exception));
      Guard.Against.NullOrWhiteSpace(message, nameof(message));

      logger.LogCritical(exception, message, args);
    }
    catch (Exception e)
    {
      logger.LogError(e, nameof(LogCritical));
      throw new LoggingException(e.Message);
    }
  }

  /// <summary>
  ///   Logs a critical message using the specified message and arguments.
  /// </summary>
  /// <param name="message">The message to log.</param>
  /// <param name="args">The arguments to format the message with.</param>
  /// <exception cref="LoggingException">Thrown if an error occurs while logging.</exception>
  public void LogCritical(string message, params object[] args)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(message, nameof(message));

      logger.LogCritical(message, args);
    }
    catch (Exception e)
    {
      logger.LogError(e, nameof(LogCritical));
      throw new LoggingException(e.Message);
    }
  }

  /// <summary>
  ///   Logs the failure of a command.
  /// </summary>
  /// <param name="commandName">The name of the command that failed.</param>
  public void LogCommandFailed(string commandName)
  {
    if (!string.IsNullOrEmpty(commandName))
    {
      LogInformation("Command has failed.");
      return;
    }

    LogInformation("{CommandName} has failed", commandName);
  }

  /// <summary>
  ///   Checks if the specified log level is enabled.
  /// </summary>
  /// <param name="level">The log level to check.</param>
  /// <returns>true if the log level is enabled; otherwise, false.</returns>
  public bool IsLogLevelEnabled(LogLevel level)
  {
    return logger.IsEnabled(level);
  }
}
