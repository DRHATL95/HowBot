using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Modules;
using Howbot.Core.Settings;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using static Howbot.Core.Models.Messages.Debug;
using static Howbot.Core.Models.Messages.Errors;

namespace Howbot.Core.Services;

public class DiscordClientService(
  DiscordSocketClient discordSocketClient,
  IServiceProvider serviceProvider,
  InteractionService interactionService,
  IVoiceService voiceService,
  IAudioService audioService,
  ILoggerAdapter<DiscordClientService> logger)
  : ServiceBase<DiscordClientService>(logger), IDiscordClientService, IDisposable
{
  private readonly IAudioService _audioService = audioService;
  private readonly IVoiceService _voiceService = voiceService;

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
    if (Log.Logger.IsEnabled(LogEventLevel.Debug))
    {
      Logger.LogDebug("{ServiceName} is initializing...", nameof(DiscordClientService));
    }

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

  public async ValueTask<bool> LoginDiscordBotAsync(string discordToken)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(discordToken));

    try
    {
      await discordSocketClient.LoginAsync(TokenType.Bot, discordToken).ConfigureAwait(false);

      return true;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, DiscordClientLogin);
    }

    return false;
  }

  public async ValueTask<bool> StartDiscordBotAsync()
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

      // Add modules dynamically to discord bot
      await AddModulesToDiscordBotAsync().ConfigureAwait(false);

      return true;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, DiscordStart);
    }

    return false;
  }

  public void Dispose()
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

  private async Task AddModulesToDiscordBotAsync()
  {
    try
    {
      await interactionService.AddModuleAsync(typeof(MusicModule), serviceProvider).ConfigureAwait(false);
      await interactionService.AddModuleAsync(typeof(AdminModule), serviceProvider).ConfigureAwait(false);
      await interactionService.AddModuleAsync(typeof(GeneralModule), serviceProvider).ConfigureAwait(false);
      await interactionService.AddModuleAsync(typeof(GameModule), serviceProvider).ConfigureAwait(false);
      await interactionService.AddModuleAsync(typeof(VideoModule), serviceProvider).ConfigureAwait(false);
    }
    catch (FileNotFoundException exception)
    {
      Logger.LogError(exception, "Unable to find the assembly. Value: {AssemblyName}",
        Assembly.GetEntryAssembly()?.ToString());
      throw;
    }
    catch (Exception e)
    {
      Logger.LogError(e, nameof(AddModulesToDiscordBotAsync));
      throw;
    }
  }

  #region Discord Client Events

  private Task DiscordSocketClientOnLog(LogMessage arg)
  {
    var severity = arg.Severity switch
    {
      LogSeverity.Critical => LogLevel.Critical,
      LogSeverity.Error => LogLevel.Error,
      LogSeverity.Warning => LogLevel.Warning,
      LogSeverity.Info => LogLevel.Information,
      LogSeverity.Verbose => LogLevel.Trace,
      LogSeverity.Debug => LogLevel.Debug,
      _ => LogLevel.Information
    };

    var message = arg.Message ?? arg.Exception?.Message ?? "No message provided";

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
    var guild = discordSocketClient.Guilds.FirstOrDefault(x => x.Id == arg.GuildId);
    if (guild == null)
    {
      Logger.LogError(new Exception(), "Unable to look-up guild for event [{EventName}]",
        nameof(DiscordSocketClientOnSlashCommandExecuted));
    }

    Logger.LogDebug("Command [{CommandName}] has been executed in Guild {GuildTag}", arg.CommandName,
      DiscordHelper.GetGuildTag(guild));

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

    await Task.Run(async () =>
    {
      await discordSocketClient.Rest.DeleteAllGlobalCommandsAsync().ConfigureAwait(false);
    });

    try
    {
      if (Configuration.IsDebug())
      {
        Logger.LogDebug("Registering commands to DEV Guild.");
        await interactionService.RegisterCommandsToGuildAsync(Constants.DiscordDevelopmentGuildId);
      }
      else
      {
        Logger.LogDebug("Registering commands globally.");
        await interactionService.RegisterCommandsGloballyAsync();
      }

      Logger.LogDebug("Successfully registered commands to discord bot.");
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

  private Task DiscordSocketClientOnUserVoiceStateUpdated(SocketUser user,
    SocketVoiceState oldVoiceState,
    SocketVoiceState newVoiceState)
  {
    // Don't care about bot voice state
    if (user.IsBot && user.Id == discordSocketClient.CurrentUser.Id)
    {
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
      var result = await interactionService.ExecuteCommandAsync(context, serviceProvider);

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

  #endregion Discord Client Events
}
