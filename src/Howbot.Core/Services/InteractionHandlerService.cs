using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public class InteractionHandlerService(
  DiscordSocketClient discordSocketClient,
  InteractionService interactionService,
  IServiceProvider serviceProvider,
  ILoggerAdapter<InteractionHandlerService> logger)
  : ServiceBase<InteractionHandlerService>(logger), IInteractionHandlerService, IDisposable
{
  private readonly DiscordSocketClient _discordSocketClient = discordSocketClient;
  private readonly IServiceProvider _serviceProvider = serviceProvider;

  public void Dispose()
  {
    interactionService.Log -= InteractionServiceOnLog;
    interactionService.InteractionExecuted -= InteractionServiceOnInteractionExecuted;

    GC.SuppressFinalize(this);
  }

  public override void Initialize()
  {
    Logger.LogDebug("{ServiceName} is initializing...", nameof(InteractionHandlerService));

    interactionService.Log += InteractionServiceOnLog;
    interactionService.InteractionExecuted += InteractionServiceOnInteractionExecuted;
    // _interactionService.SlashCommandExecuted += InteractionServiceOnSlashCommandExecuted;
    // _interactionService.ContextCommandExecuted += InteractionServiceOnContextCommandExecuted;
    // _interactionService.AutocompleteCommandExecuted += InteractionServiceOnAutocompleteCommandExecuted;
    // _interactionService.AutocompleteHandlerExecuted += InteractionServiceOnAutocompleteHandlerExecuted;
  }

  private Task InteractionServiceOnAutocompleteHandlerExecuted(IAutocompleteHandler arg1, IInteractionContext arg2,
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

      Logger.Log(logLevel, message: logMessage.Message);

      return Task.CompletedTask;
    }
    catch (ArgumentOutOfRangeException exception)
    {
      HandleException(exception, nameof(InteractionServiceOnLog));

      return Task.FromException(exception);
    }
    catch (Exception exception)
    {
      HandleException(exception, nameof(InteractionServiceOnLog));

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
      HandleException(exception, nameof(InteractionServiceOnInteractionExecuted));
      throw;
    }
  }
}
