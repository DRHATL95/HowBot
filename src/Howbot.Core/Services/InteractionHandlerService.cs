using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public class InteractionHandlerService : IInteractionHandlerService
{
  private readonly DiscordSocketClient _discordSocketClient;
  private readonly InteractionService _interactionService;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILoggerAdapter<InteractionHandlerService> _logger;

  public InteractionHandlerService(DiscordSocketClient discordSocketClient, InteractionService interactionService,
    IServiceProvider serviceProvider, ILoggerAdapter<InteractionHandlerService> logger)
  {
    _discordSocketClient = discordSocketClient;
    _interactionService = interactionService;
    _serviceProvider = serviceProvider;
    _logger = logger;
    
    // TODO: dhoward - Maybe move to discord client service..
    _discordSocketClient.InteractionCreated += DiscordSocketClientOnInteractionCreated;
    
    _interactionService.Log += InteractionServiceOnLog;
    _interactionService.InteractionExecuted += InteractionServiceOnInteractionExecuted;
    _interactionService.SlashCommandExecuted += InteractionServiceOnSlashCommandExecuted;
  }

  #region Interaction Service Events

  private Task InteractionServiceOnSlashCommandExecuted(SlashCommandInfo slashCommandInfo, IInteractionContext interactionContext, IResult result)
  {
    if (!result.IsSuccess)
    {
      if (result.Error != null)
      {
        _logger.LogError(new Exception(result.ErrorReason), "{MethodName} has thrown exception", nameof(InteractionServiceOnSlashCommandExecuted));
        return Task.CompletedTask;
      }
      
      _logger.LogInformation("Slash command was not successful, but error was not thrown");
      return Task.CompletedTask;
    }
    
    _logger.LogDebug("Slash command executed successfully");
    return Task.CompletedTask;
  }

  private Task InteractionServiceOnInteractionExecuted(ICommandInfo commandInfo, IInteractionContext interactionContext, IResult result)
  {
    if (!result.IsSuccess)
    {
      if (result.Error != null)
      {
        _logger.LogError(new Exception(result.ErrorReason), "Exception has been thrown executing interaction");
        return Task.CompletedTask;
      }
      
      _logger.LogInformation("Interaction command did not run successfully, but error was not thrown");
      return Task.CompletedTask;
    }
    
    // Command success
    _logger.LogDebug("Command [{CommandName}] has executed successfully.");
    return Task.CompletedTask;
  }

  private Task InteractionServiceOnLog(LogMessage arg)
  {
    try
    {
      var logLevel = DiscordHelper.ConvertLogSeverityToLogLevel(arg.Severity);

      if (logLevel == LogLevel.Error)
      {
        _logger.LogError(arg.Exception, "Exception thrown logging interaction service");
      }
      else
      {
        var logMessage = arg.Message ?? string.Empty;
        _logger.Log(logLevel, logMessage);
      }
      
      return Task.CompletedTask;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(InteractionServiceOnLog));
      throw;
    }
  }

  private async Task DiscordSocketClientOnInteractionCreated(SocketInteraction arg)
  {
    try
    {
      var context = new SocketInteractionContext(_discordSocketClient, arg);
      var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

      if (!result.IsSuccess)
      {
        // Error
        switch (result.Error)
        {
          case InteractionCommandError.UnknownCommand:
            _logger.LogError(Messages.Errors.InteractionUnknownCommandLog);
            
            await arg.RespondAsync(Messages.Errors.InteractionUnknownCommand, ephemeral: true);
            break;
          case InteractionCommandError.ConvertFailed:
            _logger.LogError(Messages.Errors.InteractionConvertFailedLog);
            
            await arg.RespondAsync(Messages.Errors.InteractionConvertFailed, ephemeral: true);
            break;
          case InteractionCommandError.BadArgs:
            _logger.LogError(Messages.Errors.InteractionBadArgumentsLog);

            await arg.RespondAsync(Messages.Errors.InteractionBadArguments);
            break;
          case InteractionCommandError.Exception:
            _logger.LogError(new Exception(result.ErrorReason), Messages.Errors.InteractionException);

            await arg.RespondAsync(Messages.Errors.InteractionExceptionLog, ephemeral: true);
            break;
          case InteractionCommandError.Unsuccessful:
            _logger.LogError(Messages.Errors.InteractionUnsuccessfulLog);

            await arg.RespondAsync(Messages.Errors.InteractionUnsuccessful, ephemeral: true);
            break;
          case InteractionCommandError.UnmetPrecondition:
            _logger.LogError(Messages.Errors.InteractionUnmetPreconditionLog);

            await arg.RespondAsync(Messages.Errors.InteractionUnmetPrecondition, ephemeral: true);
            break;
          case InteractionCommandError.ParseFailed:
            _logger.LogError(Messages.Errors.InteractionParseFailedLog);

            await arg.RespondAsync(Messages.Errors.InteractionParseFailed, ephemeral: true);
            break;
          case null:
            _logger.LogError(Messages.Errors.InteractionNullLog);

            await arg.RespondAsync(Messages.Errors.InteractionNull, ephemeral: true);
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "An exception has been thrown trying to run an interaction command");

      if (arg.Type is InteractionType.ApplicationCommand)
      {
        // If exception is thrown, acknowledgement will still be there. This will clean-up.
        await arg.GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
      }
    }
  }

  #endregion
}
