using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public class InteractionHandlerService(
  InteractionService interactionService,
  ILoggerAdapter<InteractionHandlerService> logger)
  : ServiceBase<InteractionHandlerService>(logger), IInteractionHandlerService, IDisposable
{
  public void Dispose()
  {
    interactionService.Log -= InteractionServiceOnLog;
    interactionService.InteractionExecuted -= InteractionServiceOnInteractionExecuted;

    GC.SuppressFinalize(this);
  }

  public override void Initialize()
  {
    base.Initialize();

    interactionService.Log += InteractionServiceOnLog;
    interactionService.InteractionExecuted += InteractionServiceOnInteractionExecuted;
    // _interactionService.SlashCommandExecuted += InteractionServiceOnSlashCommandExecuted;
    // _interactionService.ContextCommandExecuted += InteractionServiceOnContextCommandExecuted;
    // _interactionService.AutocompleteCommandExecuted += InteractionServiceOnAutocompleteCommandExecuted;
    // _interactionService.AutocompleteHandlerExecuted += InteractionServiceOnAutocompleteHandlerExecuted;
  }

  private Task InteractionServiceOnLog(LogMessage logMessage)
  {
    try
    {
      var logLevel = DiscordHelper.ConvertLogSeverityToLogLevel(logMessage.Severity);
      if (logLevel is LogLevel.Error)
      {
        throw logMessage.Exception ?? new Exception(logMessage.Message);
      }

      Logger.Log(logLevel, logMessage.Message);

      return Task.CompletedTask;
    }
    catch (ArgumentOutOfRangeException exception)
    {
      Logger.LogError("LogMessage.Severity is not a valid LogSeverity value");

      return Task.FromException(exception);
    }
    catch (Exception exception)
    {
      Logger.LogError("An exception occurred while logging interaction service log message");

      return Task.FromException(exception);
    }
  }

  private async Task InteractionServiceOnInteractionExecuted(ICommandInfo commandInfo,
    IInteractionContext interactionContext, IResult result)
  {
    try
    {
      if (result.IsSuccess)
      {
        return;
      }

      Logger.LogError("Interaction command did not execute successfully");

      if (!string.IsNullOrEmpty(result.ErrorReason))
      {
        await interactionContext.Interaction.FollowupAsync(result.ErrorReason);
      }
      else
      {
        await interactionContext.Interaction.FollowupAsync(
          "Interaction command was not able to execute successfully. Try again later.");
      }
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "An exception occurred while handling interaction command execution");
    }
  }

  /*private Task InteractionServiceOnAutocompleteHandlerExecuted(IAutocompleteHandler arg1, IInteractionContext arg2,
    IResult arg3)
  {
    throw new NotImplementedException();
  }

  private Task InteractionServiceOnAutocompleteCommandExecuted(AutocompleteCommandInfo arg1, IInteractionContext arg2,
    IResult arg3)
  {
    throw new NotImplementedException();
  }

  private Task InteractionServiceOnContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2,
    IResult arg3)
  {
    throw new NotImplementedException();
  }

  private Task InteractionServiceOnSlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
  {
    throw new NotImplementedException();
  }*/
}
