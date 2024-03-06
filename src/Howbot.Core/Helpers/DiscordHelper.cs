using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Howbot.Core.Helpers;

public static class DiscordHelper
{
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

  public static string GetGuildTag(IGuild guild)
  {
    try
    {
      Guard.Against.Null(guild, nameof(guild));

      string guildName = guild.Name ?? "Unknown Guild";
      ulong guildId = guild.Id;

      return $"[{guildName} - {guildId}]";
    }
    catch (ArgumentException argumentException)
    {
      Log.Error(argumentException, Messages.Errors.ArgumentException);
    }
    catch (Exception exception)
    {
      Log.Error(exception, Messages.Errors.Exception);
    }

    return guild == null ? string.Empty : $"[{guild.Name} - {guild.Id}]";
  }

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

    // TODO: Maybe create config variable to change this?
    // Will respond with error reason ephemeral by default.
    await socketInteraction.RespondAsync(result.ErrorReason, ephemeral: true)
      .ConfigureAwait(false);
  }
}
