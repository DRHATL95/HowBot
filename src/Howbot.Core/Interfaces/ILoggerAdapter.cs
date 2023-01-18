using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Interfaces;

// Helps if you need to confirm logging is happening
// https://ardalis.com/testing-logging-in-aspnet-core
public interface ILoggerAdapter<T>
{
  ILogger<T> GetInstance();
  void Log(LogLevel severity, string message, params object[] args);
  void LogInformation(string message, params object[] args);
  void LogError(Exception exception);
  void LogError(string message, params object[] args);
  void LogError(Exception ex, string message, params object[] args);
  void LogDebug(string message, params object[] args);
  void LogCommandFailed(string commandName);
}
