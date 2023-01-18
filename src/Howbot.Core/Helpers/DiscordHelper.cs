using System;
using Discord;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Helpers;

public static class DiscordHelper
{
  
  /// <summary>
  /// Convert Discord <see cref="LogSeverity"/> to Microsoft Logging <see cref="LogLevel"/>.
  /// </summary>
  /// <param name="logSeverity">Discord logging severity</param>
  /// <returns></returns>
  /// <exception cref="ArgumentOutOfRangeException">Throws exception if log severity cannot convert to microsoft logging level</exception>
  public static LogLevel ConvertLogSeverityToLogLevel(LogSeverity logSeverity)
  {
    return logSeverity switch
    {
      LogSeverity.Critical => LogLevel.Critical,
      LogSeverity.Error => LogLevel.Error,
      LogSeverity.Warning => LogLevel.Warning,
      LogSeverity.Info => LogLevel.Information,
      LogSeverity.Verbose => LogLevel.Trace,
      LogSeverity.Debug => LogLevel.Debug,
      _ => throw new ArgumentOutOfRangeException(nameof(logSeverity), logSeverity, null)
    };
  }
}
