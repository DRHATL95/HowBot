using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public class InteractionHandlerService : ServiceBase<InteractionHandlerService>, IInteractionHandlerService, IDisposable
{
  [NotNull] private readonly DiscordSocketClient _discordSocketClient;
  [NotNull] private readonly InteractionService _interactionService;
  [NotNull] private readonly IServiceProvider _serviceProvider;

  public InteractionHandlerService([NotNull] DiscordSocketClient discordSocketClient,
    [NotNull] InteractionService interactionService,
    [NotNull] IServiceProvider serviceProvider, ILoggerAdapter<InteractionHandlerService> logger) : base(logger)
  {
    _discordSocketClient = discordSocketClient;
    _interactionService = interactionService;
    _serviceProvider = serviceProvider;
  }

  public void Dispose()
  {
    _interactionService.Log -= InteractionServiceOnLog;
    _interactionService.InteractionExecuted -= InteractionServiceOnInteractionExecuted;

    GC.SuppressFinalize(this);
  }

  public override void Initialize()
  {
    Logger.LogDebug("{ServiceName} is now initializing...", nameof(InteractionHandlerService));

    _interactionService.Log += InteractionServiceOnLog;
    _interactionService.InteractionExecuted += InteractionServiceOnInteractionExecuted;
    _interactionService.SlashCommandExecuted += InteractionServiceOnSlashCommandExecuted;
    _interactionService.ContextCommandExecuted += InteractionServiceOnContextCommandExecuted;
    _interactionService.AutocompleteCommandExecuted += InteractionServiceOnAutocompleteCommandExecuted;
    _interactionService.AutocompleteHandlerExecuted += InteractionServiceOnAutocompleteHandlerExecuted;
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

  [NotNull]
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

  private async Task InteractionServiceOnInteractionExecuted([NotNull] ICommandInfo commandInfo,
    [NotNull] IInteractionContext interactionContext, [NotNull] IResult result)
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
