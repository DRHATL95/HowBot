using System;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Howbot.Infrastructure;

/// <summary>
///   An ILoggerAdapter implementation that uses Microsoft.Extensions.Logging
/// </summary>
/// <typeparam name="T"></typeparam>
public class LoggerAdapter<T> : ILoggerAdapter<T>
{
  [NotNull] private readonly ILogger<LoggerAdapter<T>> _logger;

  public LoggerAdapter([NotNull] ILogger<LoggerAdapter<T>> logger)
  {
    _logger = logger;
  }

  public void Log(LogLevel severity, string message, params object[] args)
  {
    try
    {
      ArgumentException.ThrowIfNullOrEmpty(message);

      _logger.Log(severity, message, args);
    }
    catch (Exception e)
    {
      _logger.LogError(e, nameof(Log));
      throw new LoggingException(e.Message);
    }
  }

  public void LogError(Exception exception)
  {
    try
    {
      ArgumentNullException.ThrowIfNull(exception);

      _logger.LogError(exception, null);
    }
    catch (Exception e)
    {
      _logger.LogError(e, nameof(LogError));
      throw new LoggingException(e.Message);
    }
  }

  public void LogError(string message, params object[] args)
  {
    try
    {
      ArgumentException.ThrowIfNullOrEmpty(message);

      _logger.LogError(message, args);
    }
    catch (Exception e)
    {
      _logger.LogError(e, nameof(LogError));
      throw new LoggingException(e.Message);
    }
  }

  public void LogError(Exception exception, string message, params object[] args)
  {
    try
    {
      ArgumentNullException.ThrowIfNull(exception);
      ArgumentException.ThrowIfNullOrEmpty(message);

      _logger.LogError(exception, message, args);
    }
    catch (Exception e)
    {
      _logger.LogError(e, nameof(LogError));
      throw new LoggingException(e.Message);
    }
  }

  public void LogInformation(string message, params object[] args)
  {
    try
    {
      ArgumentException.ThrowIfNullOrEmpty(message);

      _logger.LogInformation(message, args);
    }
    catch (Exception e)
    {
      _logger.LogError(e, nameof(LogInformation));
      throw new LoggingException(e.Message);
    }
  }

  public void LogDebug(string message, params object[] args)
  {
    try
    {
      ArgumentException.ThrowIfNullOrEmpty(message);

      _logger.LogDebug(message, args);
    }
    catch (Exception e)
    {
      _logger.LogError(e, nameof(LogDebug));
      throw new LoggingException(e.Message);
    }
  }

  public void LogWarning(string message, params object[] args)
  {
    try
    {
      ArgumentException.ThrowIfNullOrEmpty(message);

      _logger.LogWarning(message, args);
    }
    catch (Exception e)
    {
      _logger.LogError(e, nameof(LogWarning));
      throw new LoggingException(e.Message);
    }
  }

  public void LogCritical(Exception exception, string message, params object[] args)
  {
    try
    {
      ArgumentNullException.ThrowIfNull(exception);
      ArgumentException.ThrowIfNullOrEmpty(message);

      _logger.LogCritical(exception, message, args);
    }
    catch (Exception e)
    {
      _logger.LogError(e, nameof(LogCritical));
      throw new LoggingException(e.Message);
    }
  }

  public void LogCritical(string message, params object[] args)
  {
    try
    {
      ArgumentException.ThrowIfNullOrEmpty(message);

      _logger.LogCritical(message, args);
    }
    catch (Exception e)
    {
      _logger.LogError(e, nameof(LogCritical));
      throw new LoggingException(e.Message);
    }
  }

  public void LogCommandFailed(string commandName)
  {
    if (!string.IsNullOrEmpty(commandName))
    {
      LogInformation("Command has failed.");
      return;
    }

    LogInformation("{CommandName} has failed", commandName);
  }

  public bool IsLogLevelEnabled(LogLevel level)
  {
    return _logger.IsEnabled(level);
  }
}
