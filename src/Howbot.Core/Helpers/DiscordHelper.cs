using System;
using Discord;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Helpers;

/// <summary>
/// Class of static helpers used for interacting with Discord API.
/// </summary>
public static class DiscordHelper
{
  /// <summary>
  /// Converts a log severity value to a log level.
  /// </summary>
  /// <param name="logSeverity">The log severity to convert.</param>
  /// <returns>The corresponding log level.</returns>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when the log severity is not a valid value.</exception>
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

  /// <summary>
  /// Helper function to create a tag consisting of guild name and guild id.
  /// </summary>
  /// <param name="guild">Specified guild to build</param>
  /// <returns></returns>
  public static string GetGuildTag(IGuild guild)
  {
    return guild == null ? string.Empty : $"[{guild.Name} - {guild.Id}]";
  }

}
