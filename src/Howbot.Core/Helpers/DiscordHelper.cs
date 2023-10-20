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
  ///   Convert Discord <see cref="LogSeverity" /> to Microsoft Logging <see cref="LogLevel" />.
  /// </summary>
  /// <param name="logSeverity">Discord logging severity</param>
  /// <returns></returns>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
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
  /// <param name="guild"></param>
  /// <returns></returns>
  public static string GetGuildTag(IGuild guild)
  {
    return guild == null ? string.Empty : $"[{guild.Name} - {guild.Id}]";
  }

}
