﻿using Howbot.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace Howbot.Infrastructure;

/// <summary>
/// An ILoggerAdapter implementation that uses Microsoft.Extensions.Logging
/// </summary>
/// <typeparam name="T"></typeparam>
public class LoggerAdapter<T> : ILoggerAdapter<T>
{
  private readonly ILogger<LoggerAdapter<T>> _logger;

  public LoggerAdapter(ILogger<LoggerAdapter<T>> logger)
  {
    _logger = logger;
  }

  public ILogger<T> GetInstance()
  {
    return _logger as ILogger<T>;
  }

  public void Log(LogLevel severity, string message, params object[] args)
  {
    if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
    
    _logger.Log(severity, message, args);
  }

  public void LogError(string message, params object[] args)
  {
    if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
    
    _logger.Log(LogLevel.Error, message);
  }

  public void LogError(Exception ex, string message, params object[] args)
  {
    if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
    if (ex.Equals(null)) throw new ArgumentNullException(nameof(ex));

    _logger.LogError(ex, message, args);
  }

  public void LogInformation(string message, params object[] args)
  {
    if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));

    _logger.LogInformation(message, args);
  }

  public void LogDebug(string message, params object[] args)
  {
    if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
    
    _logger.LogDebug(message, args);
  }
}
