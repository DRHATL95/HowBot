using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Application.Constants;
using Howbot.Application.Interfaces.Discord;
using Howbot.Application.Models.Discord;
using Howbot.Infrastructure.Discord.Exceptions;
using Howbot.Infrastructure.Discord.Helpers;
using Howbot.Infrastructure.Services.Abstract;
using Howbot.SharedKernel;
using Microsoft.Extensions.Logging;
using InteractionService = Discord.Interactions.InteractionService;

namespace Howbot.Infrastructure.Discord.Services;

public class DiscordClientService(
  DiscordSocketClient discordSocketClient,
  InteractionService interactionService,
  IServiceProvider serviceProvider,
  ILoggerAdapter<DiscordClientService> logger)
  : ServiceBase<DiscordClientService>(logger), IDiscordClientService, IDisposable
{
  private string _loggedInUsername = string.Empty;

  private string LoggedInUsername
  {
    get => string.IsNullOrEmpty(_loggedInUsername) ? DiscordBot.BotName : _loggedInUsername;
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

    SubscribeToDiscordSocketEvents();
  }

  public async Task LoginDiscordBotAsync(string discordToken)
  {
    try
    {
      Guard.Against.NullOrWhiteSpace(discordToken, nameof(discordToken));

      await discordSocketClient.LoginAsync(TokenType.Bot, discordToken);
    }
    catch (ArgumentException)
    {
      // Should bubble up to the caller
      throw new DiscordLoginException("Invalid token provided.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(LoginDiscordBotAsync));
      throw new DiscordLoginException("An exception has been thrown while logging into Discord.");
    }
  }

  public async Task StartDiscordBotAsync()
  {
    try
    {
      // Will signal ready state. Must be called only when bot has finished logging in.
      await discordSocketClient.StartAsync();
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, Messages.Errors.DiscordStart);
      throw;
    }
  }

  public void Dispose()
  {
    UnsubscribeFromDiscordSocketEvents();

    GC.SuppressFinalize(this);
  }

  private void SubscribeToDiscordSocketEvents()
  {
    discordSocketClient.Log += DiscordSocketClientOnLog;
    discordSocketClient.UserJoined += DiscordSocketClientOnUserJoined;
    discordSocketClient.JoinedGuild += DiscordSocketClientOnJoinedGuild;
    discordSocketClient.LoggedIn += DiscordSocketClientOnLoggedIn;
    discordSocketClient.LoggedOut += DiscordSocketClientOnLoggedOut;
    discordSocketClient.Ready += DiscordSocketClientOnReady;
    discordSocketClient.Connected += DiscordSocketClientOnConnected;
    discordSocketClient.Disconnected += DiscordSocketClientOnDisconnected;
    // discordSocketClient.SlashCommandExecuted += DiscordSocketClientOnSlashCommandExecuted; TODO: Revisit this
    discordSocketClient.UserVoiceStateUpdated += DiscordSocketClientOnUserVoiceStateUpdated;
    discordSocketClient.VoiceServerUpdated += DiscordSocketClientOnVoiceServerUpdated;
    discordSocketClient.InteractionCreated += DiscordSocketClientOnInteractionCreated;
  }

  private void UnsubscribeFromDiscordSocketEvents()
  {
    discordSocketClient.Log -= DiscordSocketClientOnLog;
    discordSocketClient.UserJoined -= DiscordSocketClientOnUserJoined;
    discordSocketClient.JoinedGuild -= DiscordSocketClientOnJoinedGuild;
    discordSocketClient.LoggedIn -= DiscordSocketClientOnLoggedIn;
    discordSocketClient.LoggedOut -= DiscordSocketClientOnLoggedOut;
    discordSocketClient.Ready -= DiscordSocketClientOnReady;
    discordSocketClient.Connected -= DiscordSocketClientOnConnected;
    discordSocketClient.Disconnected -= DiscordSocketClientOnDisconnected;
    // discordSocketClient.SlashCommandExecuted -= DiscordSocketClientOnSlashCommandExecuted; TODO: Revisit this
    discordSocketClient.UserVoiceStateUpdated -= DiscordSocketClientOnUserVoiceStateUpdated;
    discordSocketClient.VoiceServerUpdated -= DiscordSocketClientOnVoiceServerUpdated;
    discordSocketClient.InteractionCreated -= DiscordSocketClientOnInteractionCreated;
  }

  /// <summary>
  ///   Maps <see cref="LogSeverity" /> enum to <see cref="LogLevel" /> enum.
  /// </summary>
  /// <param name="severity">The Discord.NET <see cref="LogSeverity" /> level</param>
  /// <returns>The <see cref="LogLevel" /> representation</returns>
  private static LogLevel MapLogSeverity(LogSeverity severity)
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

  #region Discord Client Events

  private Task DiscordSocketClientOnLog(LogMessage arg)
  {
    var severity = MapLogSeverity(arg.Severity);

    var message = arg.Message ?? arg.Exception?.Message ?? "No message provided";

    Logger.Log(severity, message);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnUserJoined(SocketGuildUser socketGuildUser)
  {
    Logger.LogDebug("[{0}] has joined Guild [{1}]", socketGuildUser.Username,
      DiscordHelper.GetGuildTag(socketGuildUser.Guild));

    return Task.CompletedTask;
  }

  // TODO: Revisit this
  /*private Task DiscordSocketClientOnSlashCommandExecuted(SocketSlashCommand arg)
  {
    var guild = GetGuildByGuildId(arg.GuildId);
    if (guild == null)
    {
      LogErrorGuildLookup(nameof(DiscordSocketClientOnSlashCommandExecuted));
    }

    var logMessage = $"Command [{arg.CommandName}] has been executed in Guild {DiscordHelper.GetGuildTag(guild)}";
    Logger.LogDebug(logMessage);

    return Task.CompletedTask;
  }*/

  private Task DiscordSocketClientOnDisconnected(Exception exception)
  {
    Logger.LogError(exception, "[{0}] has disconnected from the WS", LoggedInUsername);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnConnected()
  {
    // Assign instance variable to the current username. By this point there should be a value for discordSocketClient.CurrentUser.Username
    LoggedInUsername = discordSocketClient.CurrentUser.Username;

    Logger.LogDebug(Messages.Debug.DiscordSocketClientConnected, LoggedInUsername);

    return Task.CompletedTask;
  }

  private async Task DiscordSocketClientOnReady()
  {
    Logger.LogDebug("[{0}] is now in READY state", LoggedInUsername);

    try
    {
#if DEBUG
      Logger.LogInformation($"Registering commands to DEV Guild [{DiscordBot.DiscordDevelopmentGuildId}]");

      await interactionService.RegisterCommandsToGuildAsync(DiscordBot.DiscordDevelopmentGuildId);
#else
      Logger.LogInformation("Registering commands globally");

        // TODO: Investigate if this is necessary
        await interactionService.RegisterCommandsGloballyAsync();
#endif
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(DiscordSocketClientOnReady));
      throw;
    }
  }

  private Task DiscordSocketClientOnLoggedOut()
  {
    Logger.LogDebug("[{0}] has logged out successfully", LoggedInUsername);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnLoggedIn()
  {
    Logger.LogDebug("[{0}] has logged in successfully", LoggedInUsername);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnJoinedGuild(SocketGuild guild)
  {
    Logger.LogDebug("[{0}] has joined Guild [{1}]", LoggedInUsername, $"{guild.Name} - {guild.Id}");

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldVoiceState,
    SocketVoiceState newVoiceState)
  {
    // Don't care about bot voice state
    if (user.IsBot && user.Id == discordSocketClient.CurrentUser.Id)
    {
      Logger.LogDebug("Voice state update ignored for bot user [{0}].", user.Username);

      return Task.CompletedTask;
    }

    Logger.LogDebug("User {0} has updated voice state.", user.Username);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnVoiceServerUpdated(SocketVoiceServer socketVoiceServer)
  {
    if (!socketVoiceServer.Guild.HasValue)
    {
      return Task.CompletedTask;
    }

    var guildTag = $"{socketVoiceServer.Guild.Value.Name} - {socketVoiceServer.Guild.Value.Id}";

    Logger.LogDebug("[{0}] has connected to server [{1}]", LoggedInUsername, guildTag);

    return Task.CompletedTask;
  }

  private async Task DiscordSocketClientOnInteractionCreated(SocketInteraction socketInteraction)
  {
    try
    {
      var context = new SocketInteractionContext(discordSocketClient, socketInteraction);
      var result = await interactionService.ExecuteCommandAsync(context, serviceProvider);

      // Due to async nature of InteractionFramework, the result here may always be success.
      // That's why we also need to handle the InteractionExecuted event.
      if (!result.IsSuccess)
      {
        await DiscordHelper.HandleSocketInteractionErrorAsync(socketInteraction, result, Logger);
      }
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "An exception has been thrown while running interaction command");

      if (socketInteraction.Type is InteractionType.ApplicationCommand)
      {
        Logger.LogInformation("Attempting to delete the failed command");

        // If exception is thrown, acknowledgement will still be there. This will clean up.
        await socketInteraction.DeleteOriginalResponseAsync();

        Logger.LogInformation("Successfully deleted the failed command");
      }
    }
  }

#endregion Discord Client Events
}
