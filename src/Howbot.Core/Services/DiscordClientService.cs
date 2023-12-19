using System;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Settings;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using static Howbot.Core.Models.Messages.Debug;
using static Howbot.Core.Models.Messages.Errors;

namespace Howbot.Core.Services;

public class DiscordClientService(
  DiscordSocketClient discordSocketClient,
  IInteractionService interactionService,
  ILoggerAdapter<DiscordClientService> logger)
  : ServiceBase<DiscordClientService>(logger), IDiscordClientService, IDisposable
{
  private string _loggedInUsername = string.Empty;

  private string LoggedInUsername
  {
    get => string.IsNullOrEmpty(_loggedInUsername) ? Constants.BotName : _loggedInUsername;
    set
    {
      if (!value.Equals(_loggedInUsername, StringComparison.OrdinalIgnoreCase))
      {
        _loggedInUsername = value;
      }
    }
  }

  public override void Initialize()
  {
    base.Initialize();

    InitializeLogger();
    AssignEventHandlerForDiscordSocketClient();
  }

  public async Task LoginDiscordBotAsync(string discordToken)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(discordToken, nameof(discordToken));

      await discordSocketClient.LoginAsync(TokenType.Bot, discordToken)
        .ConfigureAwait(false);
    }
    catch (ArgumentException argumentException)
    {
      Logger.LogError(argumentException, "The provided token is invalid.");
      throw;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "An exception has been thrown logging into Discord.");
      throw;
    }
  }

  public async Task StartDiscordBotAsync()
  {
    try
    {
      // Will signal ready state. Must be called only when bot has finished logging in.
      await discordSocketClient.StartAsync().ConfigureAwait(false);

      // Only in debug, set bots online presence to offline
      if (Configuration.IsDebug())
      {
        await discordSocketClient.SetStatusAsync(UserStatus.Invisible).ConfigureAwait(false);
      }
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, DiscordStart);
      throw;
    }
  }

  public void Dispose()
  {
    UnsubscribeFromDiscordEvents();
    GC.SuppressFinalize(this);
  }

  private void InitializeLogger()
  {
    if (Log.Logger.IsEnabled(LogEventLevel.Debug))
    {
      Logger.LogDebug("{ServiceName} is initializing...", nameof(DiscordClientService));
    }
  }

  private void UnsubscribeFromDiscordEvents()
  {
    discordSocketClient.Log -= DiscordSocketClientOnLog;
    discordSocketClient.UserJoined -= DiscordSocketClientOnUserJoined;
    discordSocketClient.JoinedGuild -= DiscordSocketClientOnJoinedGuild;
    discordSocketClient.LoggedIn -= DiscordSocketClientOnLoggedIn;
    discordSocketClient.LoggedOut -= DiscordSocketClientOnLoggedOut;
    discordSocketClient.Ready -= DiscordSocketClientOnReady;
    discordSocketClient.Connected -= DiscordSocketClientOnConnected;
    discordSocketClient.Disconnected -= DiscordSocketClientOnDisconnected;
    discordSocketClient.SlashCommandExecuted -= DiscordSocketClientOnSlashCommandExecuted;
    discordSocketClient.UserVoiceStateUpdated -= DiscordSocketClientOnUserVoiceStateUpdated;
    discordSocketClient.VoiceServerUpdated -= DiscordSocketClientOnVoiceServerUpdated;
    discordSocketClient.InteractionCreated -= DiscordSocketClientOnInteractionCreated;
  }

  private void AssignEventHandlerForDiscordSocketClient()
  {
    discordSocketClient.Log += DiscordSocketClientOnLog;
    discordSocketClient.UserJoined += DiscordSocketClientOnUserJoined;
    discordSocketClient.JoinedGuild += DiscordSocketClientOnJoinedGuild;
    discordSocketClient.LoggedIn += DiscordSocketClientOnLoggedIn;
    discordSocketClient.LoggedOut += DiscordSocketClientOnLoggedOut;
    discordSocketClient.Ready += DiscordSocketClientOnReady;
    discordSocketClient.Connected += DiscordSocketClientOnConnected;
    discordSocketClient.Disconnected += DiscordSocketClientOnDisconnected;
    discordSocketClient.SlashCommandExecuted += DiscordSocketClientOnSlashCommandExecuted;
    discordSocketClient.UserVoiceStateUpdated += DiscordSocketClientOnUserVoiceStateUpdated;
    discordSocketClient.VoiceServerUpdated += DiscordSocketClientOnVoiceServerUpdated;
    discordSocketClient.InteractionCreated += DiscordSocketClientOnInteractionCreated;
  }

  private LogLevel MapLogSeverity(LogSeverity severity)
  {
    return severity switch
    {
      LogSeverity.Critical => LogLevel.Critical,
      LogSeverity.Error => LogLevel.Error,
      LogSeverity.Warning => LogLevel.Warning,
      LogSeverity.Info => LogLevel.Information,
      LogSeverity.Verbose => LogLevel.Trace,
      LogSeverity.Debug => LogLevel.Debug,
      _ => LogLevel.Information
    };
  }

  private string CreateLogMessage(LogMessage log)
  {
    return log.Message ?? log.Exception?.Message ?? "No message provided";
  }

  private SocketGuild GetGuildById(ulong? guildId)
  {
    return guildId != null ? discordSocketClient.Guilds.FirstOrDefault(x => x.Id == guildId) : null;
  }

  private void LogErrorGuildLookup(string eventName)
  {
    var exception = new Exception();
    var message = $"Unable to look-up guild for event [{eventName}]";
    Logger.LogError(exception, message);
  }

  private Func<Task> GetRegisterCommandAction()
  {
    if (Configuration.IsDebug())
    {
      Logger.LogDebug("Registering commands to DEV Guild.");
      return () => interactionService.RegisterCommandsToGuildAsync(Constants.DiscordDevelopmentGuildId);
    }

    Logger.LogDebug("Registering commands globally.");
    return interactionService.RegisterCommandsGloballyAsync;
  }

  #region Discord Client Events

  private Task DiscordSocketClientOnLog(LogMessage arg)
  {
    var severity = MapLogSeverity(arg.Severity);
    var message = CreateLogMessage(arg);

    Logger.Log(severity, message);
    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnUserJoined(SocketGuildUser arg)
  {
    Logger.LogDebug("{GuildUserName} has joined Guild {GuildTag}", arg.Username, DiscordHelper.GetGuildTag(arg.Guild));

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnSlashCommandExecuted(SocketSlashCommand arg)
  {
    var guild = GetGuildById(arg.GuildId);
    if (guild == null)
    {
      LogErrorGuildLookup(nameof(DiscordSocketClientOnSlashCommandExecuted));
    }

    var logMessage = $"Command [{arg.CommandName}] has been executed in Guild {DiscordHelper.GetGuildTag(guild)}";
    Logger.LogDebug(logMessage);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnDisconnected(Exception arg)
  {
    Logger.LogError(arg, "{Username} has disconnected from the ws.", LoggedInUsername);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnConnected()
  {
    LoggedInUsername = discordSocketClient.CurrentUser.Username;

    Logger.LogDebug(DiscordSocketClientConnected, LoggedInUsername);

    return Task.CompletedTask;
  }

  private async Task DiscordSocketClientOnReady()
  {
    Logger.LogDebug("{Username} is now in READY state", LoggedInUsername);

    await discordSocketClient.Rest.DeleteAllGlobalCommandsAsync().ConfigureAwait(false);

    try
    {
      var registerCommandAction = GetRegisterCommandAction();
      await registerCommandAction();

      Logger.LogDebug(RegisteredCommandsMessage);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(DiscordSocketClientOnReady));
      throw;
    }
  }

  private Task DiscordSocketClientOnLoggedOut()
  {
    Logger.LogDebug("{Username} has logged out successfully", LoggedInUsername);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnLoggedIn()
  {
    Logger.LogDebug("{Username} has logged in successfully", LoggedInUsername);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnJoinedGuild(SocketGuild arg)
  {
    Logger.LogDebug("{Username} has joined Guild {GuildTag}", LoggedInUsername, DiscordHelper.GetGuildTag(arg));

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldVoiceState,
    SocketVoiceState newVoiceState)
  {
    // Don't care about bot voice state
    if (user.IsBot && user.Id == discordSocketClient.CurrentUser.Id)
    {
      Logger.LogDebug("Voice state update ignored for bot user {Username}.", user.Username);
      return Task.CompletedTask;
    }

    Logger.LogDebug("User {Username} has updated voice state.", user.Username);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnVoiceServerUpdated(SocketVoiceServer arg)
  {
    Logger.LogDebug("Bot has connected to server {X}", DiscordHelper.GetGuildTag(arg.Guild.Value));

    return Task.CompletedTask;
  }

  private async Task DiscordSocketClientOnInteractionCreated(SocketInteraction socketInteraction)
  {
    try
    {
      var context = new SocketInteractionContext(discordSocketClient, socketInteraction);
      var result = await interactionService.ExecuteCommandAsync(context);

      if (!result.IsSuccess)
      {
        // Error
        switch (result.Error)
        {
          case InteractionCommandError.UnknownCommand:
            Logger.LogError(InteractionUnknownCommandLog);

            await socketInteraction.RespondAsync(InteractionUnknownCommand, ephemeral: true);
            break;

          case InteractionCommandError.ConvertFailed:
            Logger.LogError(InteractionConvertFailedLog);

            await socketInteraction.RespondAsync(InteractionConvertFailed, ephemeral: true);
            break;

          case InteractionCommandError.BadArgs:
            Logger.LogError(InteractionBadArgumentsLog);

            await socketInteraction.RespondAsync(InteractionBadArguments);
            break;

          case InteractionCommandError.Exception:
            Logger.LogError(new Exception(result.ErrorReason), InteractionException);

            await socketInteraction.RespondAsync(InteractionExceptionLog, ephemeral: true);
            break;

          case InteractionCommandError.Unsuccessful:
            Logger.LogError(InteractionUnsuccessfulLog);

            await socketInteraction.RespondAsync(InteractionUnsuccessful, ephemeral: true);
            break;

          case InteractionCommandError.UnmetPrecondition:
            Logger.LogError(InteractionUnmetPreconditionLog);

            await socketInteraction.RespondAsync(InteractionUnmetPrecondition, ephemeral: true);
            break;

          case InteractionCommandError.ParseFailed:
            Logger.LogError(InteractionParseFailedLog);

            await socketInteraction.RespondAsync(InteractionParseFailed, ephemeral: true);
            break;

          case null:
            Logger.LogError(InteractionNullLog);

            await socketInteraction.RespondAsync(InteractionNull, ephemeral: true);
            break;

          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "An exception has been thrown while running interaction command");

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

  #endregion Discord Client Events
}
