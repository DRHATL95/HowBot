using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
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

    _discordSocketClient.InteractionCreated += DiscordSocketClientOnInteractionCreated;

    _interactionService.Log += InteractionServiceOnLog;
    _interactionService.InteractionExecuted += InteractionServiceOnInteractionExecuted;
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

  private async Task DiscordSocketClientOnInteractionCreated([NotNull] SocketInteraction socketInteraction)
  {
    try
    {
      var context = new SocketInteractionContext(_discordSocketClient, socketInteraction);
      var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

      if (!result.IsSuccess)
      {
        // Error
        switch (result.Error)
        {
          case InteractionCommandError.UnknownCommand:
            Logger.LogError(Messages.Errors.InteractionUnknownCommandLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionUnknownCommand, ephemeral: true);
            break;

          case InteractionCommandError.ConvertFailed:
            Logger.LogError(Messages.Errors.InteractionConvertFailedLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionConvertFailed, ephemeral: true);
            break;

          case InteractionCommandError.BadArgs:
            Logger.LogError(Messages.Errors.InteractionBadArgumentsLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionBadArguments);
            break;

          case InteractionCommandError.Exception:
            Logger.LogError(new Exception(result.ErrorReason), Messages.Errors.InteractionException);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionExceptionLog, ephemeral: true);
            break;

          case InteractionCommandError.Unsuccessful:
            Logger.LogError(Messages.Errors.InteractionUnsuccessfulLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionUnsuccessful, ephemeral: true);
            break;

          case InteractionCommandError.UnmetPrecondition:
            Logger.LogError(Messages.Errors.InteractionUnmetPreconditionLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionUnmetPrecondition, ephemeral: true);
            break;

          case InteractionCommandError.ParseFailed:
            Logger.LogError(Messages.Errors.InteractionParseFailedLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionParseFailed, ephemeral: true);
            break;

          case null:
            Logger.LogError(Messages.Errors.InteractionNullLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionNull, ephemeral: true);
            break;

          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }
    catch (Exception exception)
    {
      HandleException(exception, nameof(DiscordSocketClientOnInteractionCreated));

      if (socketInteraction.Type is InteractionType.ApplicationCommand)
      {
        Logger.LogInformation("Attempting to delete the failed command..");

        // If exception is thrown, acknowledgement will still be there. This will clean-up.
        await socketInteraction.GetOriginalResponseAsync().ContinueWith(async task =>
          await task.Result.DeleteAsync().ConfigureAwait(false)
        );

        Logger.LogInformation("Successfully deleted the failed command.");
      }
    }
  }

  private async Task InteractionServiceOnInteractionExecuted([NotNull] ICommandInfo commandInfo,
    [NotNull] IInteractionContext interactionContext, [NotNull] IResult result)
  {
    try
    {
    }
    catch (Exception exception)
    {
      HandleException(exception, nameof(InteractionServiceOnInteractionExecuted));
      throw;
    }

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
}
