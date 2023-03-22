using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Victoria.Node;
using Victoria.Player;

namespace Howbot.Core.Services;

public class InteractionHandlerService : ServiceBase<InteractionHandlerService>, IInteractionHandlerService
{
  private readonly DiscordSocketClient _discordSocketClient;
  private readonly InteractionService _interactionService;
  private readonly LavaNode<Player<LavaTrack>, LavaTrack> _lavaNode;
  private readonly ILoggerAdapter<InteractionHandlerService> _logger;
  private readonly IServiceProvider _serviceProvider;

  public InteractionHandlerService(DiscordSocketClient discordSocketClient, InteractionService interactionService,
    IServiceProvider serviceProvider, LavaNode<Player<LavaTrack>, LavaTrack> lavaNode,
    ILoggerAdapter<InteractionHandlerService> logger) : base(logger)
  {
    _discordSocketClient = discordSocketClient;
    _interactionService = interactionService;
    _serviceProvider = serviceProvider;
    _lavaNode = lavaNode;
    _logger = logger;
  }

  public new void Initialize()
  {
    if (_discordSocketClient == null)
    {
      return;
    }

    if (_interactionService == null)
    {
      return;
    }

    if (_logger.IsLogLevelEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("{ServiceName} is initializing..", nameof(InteractionHandlerService));
    }

    _discordSocketClient.InteractionCreated += DiscordSocketClientOnInteractionCreated;

    _interactionService.Log += InteractionServiceOnLog;
  }

  #region Interaction Service Events

  private Task InteractionServiceOnLog(LogMessage logMessage)
  {
    try
    {
      var logLevel = DiscordHelper.ConvertLogSeverityToLogLevel(logMessage.Severity);

      if (logLevel == LogLevel.Error)
      {
        _logger.LogError(logMessage.Exception, "Exception thrown logging interaction service");
      }
      else
      {
        _logger.Log(logLevel, (logMessage.Message ?? string.Empty));
      }

      return Task.CompletedTask;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(InteractionServiceOnLog));
      throw;
    }
  }

  private async Task DiscordSocketClientOnInteractionCreated(SocketInteraction socketInteraction)
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
            _logger.LogError(Messages.Errors.InteractionUnknownCommandLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionUnknownCommand, ephemeral: true);
            break;
          case InteractionCommandError.ConvertFailed:
            _logger.LogError(Messages.Errors.InteractionConvertFailedLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionConvertFailed, ephemeral: true);
            break;
          case InteractionCommandError.BadArgs:
            _logger.LogError(Messages.Errors.InteractionBadArgumentsLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionBadArguments);
            break;
          case InteractionCommandError.Exception:
            _logger.LogError(new Exception(result.ErrorReason), Messages.Errors.InteractionException);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionExceptionLog, ephemeral: true);
            break;
          case InteractionCommandError.Unsuccessful:
            _logger.LogError(Messages.Errors.InteractionUnsuccessfulLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionUnsuccessful, ephemeral: true);
            break;
          case InteractionCommandError.UnmetPrecondition:
            _logger.LogError(Messages.Errors.InteractionUnmetPreconditionLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionUnmetPrecondition, ephemeral: true);
            break;
          case InteractionCommandError.ParseFailed:
            _logger.LogError(Messages.Errors.InteractionParseFailedLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionParseFailed, ephemeral: true);
            break;
          case null:
            _logger.LogError(Messages.Errors.InteractionNullLog);

            await socketInteraction.RespondAsync(Messages.Errors.InteractionNull, ephemeral: true);
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "An exception has been thrown trying to run an interaction command");

      if (socketInteraction.Type is InteractionType.ApplicationCommand)
      {
        // If exception is thrown, acknowledgement will still be there. This will clean-up.
        await socketInteraction.GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
      }
    }
  }

  #endregion
}
