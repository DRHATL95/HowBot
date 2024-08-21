using Microsoft.Extensions.Logging;

namespace Howbot.Core.Interfaces;

public interface ILoggerAdapter<T>
{
  void Log(LogLevel severity, string message, params object[] args);

  void LogInformation(string message, params object[] args);

  void LogError(Exception exception);

  void LogError(string message, params object[] args);

  void LogError(Exception ex, string message, params object[] args);

  void LogDebug(string message, params object[] args);

  void LogWarning(string message, params object[] args);

  void LogCritical(Exception exception, string message, params object[] args);

  void LogCritical(string message, params object[] args);

  void LogCommandFailed(string commandName);

  bool IsLogLevelEnabled(LogLevel level);
}
