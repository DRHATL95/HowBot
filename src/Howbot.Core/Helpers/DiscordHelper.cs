using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Helpers;

public static class DiscordHelper
{
  /// <summary>
  ///   Converts a log severity value to a log level.
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
  ///   Helper function to create a tag consisting of guild name and guild id.
  /// </summary>
  /// <param name="guild">Specified guild to build</param>
  /// <returns></returns>
  public static string GetGuildTag(IGuild guild)
  {
    return guild == null ? string.Empty : $"[{guild.Name} - {guild.Id}]";
  }

  /// <summary>
  ///   TODO: Add summary.
  /// </summary>
  /// <param name="socketInteraction"></param>
  /// <param name="result"></param>
  /// <param name="logger"></param>
  /// <typeparam name="T"></typeparam>
  /// <exception cref="ArgumentOutOfRangeException"></exception>
  public static async Task HandleSocketInteractionErrorAsync<T>(SocketInteraction socketInteraction, IResult result,
    ILoggerAdapter<T> logger)
  {
    Guard.Against.Null(socketInteraction, nameof(socketInteraction));
    Guard.Against.Null(result, nameof(result));
    Guard.Against.Null(logger, nameof(logger));

    switch (result.Error)
    {
      case InteractionCommandError.UnknownCommand:
        logger.LogWarning(Messages.Warnings.InteractionUnknownCommandLog);
        break;

      case InteractionCommandError.ConvertFailed:
        logger.LogWarning(Messages.Warnings.InteractionConvertFailedLog);
        break;

      case InteractionCommandError.BadArgs:
        logger.LogWarning(Messages.Warnings.InteractionBadArgumentsLog);
        break;

      case InteractionCommandError.Exception:
        logger.LogError(new Exception(result.ErrorReason), Messages.Errors.InteractionException);
        break;

      case InteractionCommandError.Unsuccessful:
        logger.LogWarning(Messages.Errors.InteractionUnsuccessfulLog);
        break;

      case InteractionCommandError.UnmetPrecondition:
        logger.LogWarning(Messages.Errors.InteractionUnmetPreconditionLog);
        break;

      case InteractionCommandError.ParseFailed:
        logger.LogWarning(Messages.Errors.InteractionParseFailedLog);
        break;

      case null:
        logger.LogError(new Exception(result.ErrorReason), Messages.Errors.InteractionNullLog);

        break;

      default:
        throw new ArgumentOutOfRangeException();
    }

    // Will respond with error reason ephemeral by default. TODO: Maybe create config variable to change this?
    await socketInteraction.RespondAsync(result.ErrorReason, ephemeral: true);
  }
}
