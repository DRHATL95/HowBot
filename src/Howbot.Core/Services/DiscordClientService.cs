using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Modules;
using Howbot.Core.Settings;
using JetBrains.Annotations;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using static Howbot.Core.Models.Messages.Debug;
using static Howbot.Core.Models.Messages.Errors;

namespace Howbot.Core.Services;

public class DiscordClientService : ServiceBase<DiscordClientService>, IDiscordClientService
{
  [NotNull] private readonly DiscordSocketClient _discordSocketClient;
  [NotNull] private readonly InteractionService _interactionService;
  [NotNull] private readonly IAudioService _audioService;
  [NotNull] private readonly ILoggerAdapter<DiscordClientService> _logger;
  [NotNull] private readonly IServiceProvider _serviceProvider;
  [NotNull] private readonly IVoiceService _voiceService;

  [NotNull] private string _loggedInUsername = string.Empty;
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

  public DiscordClientService([NotNull] DiscordSocketClient discordSocketClient, [NotNull] IServiceProvider serviceProvider, [NotNull] InteractionService interactionService,
    [NotNull] IVoiceService voiceService, [NotNull] IAudioService audioService, [NotNull] ILoggerAdapter<DiscordClientService> logger) : base(logger)
  {
    _discordSocketClient = discordSocketClient;
    _serviceProvider = serviceProvider;
    _interactionService = interactionService;
    _voiceService = voiceService;
    _audioService = audioService;
    _logger = logger;
  }

  public new void Initialize()
  {
    if (_logger.IsLogLevelEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("{ServiceName} is initializing..", typeof(DiscordClientService).ToString());
    }

    // Hook up discord client events to this service
    _discordSocketClient.Log += DiscordSocketClientOnLog;
    _discordSocketClient.UserJoined += DiscordSocketClientOnUserJoined;
    _discordSocketClient.JoinedGuild += DiscordSocketClientOnJoinedGuild;
    _discordSocketClient.LoggedIn += DiscordSocketClientOnLoggedIn;
    _discordSocketClient.LoggedOut += DiscordSocketClientOnLoggedOut;
    _discordSocketClient.Ready += DiscordSocketClientOnReady;
    _discordSocketClient.Connected += DiscordSocketClientOnConnected;
    _discordSocketClient.Disconnected += DiscordSocketClientOnDisconnected;
    _discordSocketClient.SlashCommandExecuted += DiscordSocketClientOnSlashCommandExecuted;
    _discordSocketClient.UserVoiceStateUpdated += DiscordSocketClientOnUserVoiceStateUpdated;
  }

  public async ValueTask<bool> LoginDiscordBotAsync(string discordToken)
  {
    ArgumentException.ThrowIfNullOrEmpty(nameof(discordToken));

    try
    {
      await _discordSocketClient.LoginAsync(TokenType.Bot, discordToken).ConfigureAwait(false);

      return true;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, DiscordClientLogin);
      return false;
    }
  }

  public async ValueTask<bool> StartDiscordBotAsync()
  {
    try
    {
      // Will signal ready state. Must be called only when bot has finished logging in.
      await _discordSocketClient.StartAsync().ConfigureAwait(false);

      // Only in debug, set bots online presence to offline
      if (Configuration.IsDebug())
      {
        await _discordSocketClient.SetStatusAsync(UserStatus.Invisible).ConfigureAwait(false);
      }

      // Add modules dynamically to discord bot
      await AddModulesToDiscordBotAsync().ConfigureAwait(false);

      return true;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, DiscordStart);
      throw;
    }
  }

  private async Task AddModulesToDiscordBotAsync()
  {
    try
    {
      await _interactionService.AddModuleAsync(typeof(MusicModule), _serviceProvider);
      // await _interactionService.AddModuleAsync(typeof(GeneralModule), _serviceProvider);
    }
    catch (FileNotFoundException exception)
    {
      _logger.LogError(exception, "Unable to find the assembly. Value: {AssemblyName}",
        Assembly.GetEntryAssembly()?.ToString());
      throw;
    }
    catch (Exception e)
    {
      _logger.LogError(e);
      throw;
    }
  }

  #region Discord Client Events

  [NotNull]
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

    _logger.Log(severity, arg.Message);

    return Task.CompletedTask;
  }

  [NotNull] 
  private Task DiscordSocketClientOnUserJoined(SocketGuildUser arg)
  {
    _logger.LogDebug("{GuildUserName} has joined Guild {GuildTag}", arg.Username, DiscordHelper.GetGuildTag(arg.Guild));

    return Task.CompletedTask;
  }

  [NotNull]
  private Task DiscordSocketClientOnSlashCommandExecuted(SocketSlashCommand arg)
  {
    var guild = _discordSocketClient.Guilds.FirstOrDefault(x => x.Id == arg.GuildId);
    if (guild == null)
    {
      _logger.LogError(new Exception(), "Unable to look-up guild for event [{EventName}]",
        nameof(DiscordSocketClientOnSlashCommandExecuted));
    }

    _logger.LogDebug("Command [{CommandName}] has been executed in Guild {GuildTag}", arg.CommandName,
      DiscordHelper.GetGuildTag(guild));

    return Task.CompletedTask;
  }

  [NotNull]
  private Task DiscordSocketClientOnDisconnected(Exception arg)
  {
    _logger.LogDebug("{Username} has disconnected from socket", LoggedInUsername);

    return Task.CompletedTask;
  }

  [NotNull]
  private Task DiscordSocketClientOnConnected()
  {
    LoggedInUsername = _discordSocketClient.CurrentUser.Username;

    _logger.LogDebug(DiscordSocketClientConnected, LoggedInUsername);

    return Task.CompletedTask;
  }

  private async Task DiscordSocketClientOnReady()
  {
    _logger.LogDebug("{Username} is now in READY state", LoggedInUsername);

    try
    {
      if (Configuration.IsDebug())
      {
        _logger.LogDebug("Registering commands to DEV Guild.");
        await _interactionService.RegisterCommandsToGuildAsync(Constants.DiscordDevelopmentGuildId);
      }
      else
      {
        _logger.LogDebug("Registering commands globally.");
        await _interactionService.RegisterCommandsGloballyAsync();
      }

      _logger.LogDebug("Successfully registered commands to discord bot.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(DiscordSocketClientOnReady));
      throw;
    }
  }

  [NotNull]
  private Task DiscordSocketClientOnLoggedOut()
  {
    _logger.LogDebug("{Username} has logged out successfully", LoggedInUsername);

    return Task.CompletedTask;
  }

  [NotNull]
  private Task DiscordSocketClientOnLoggedIn()
  {
    _logger.LogDebug("{Username} has logged in successfully", LoggedInUsername);

    return Task.CompletedTask;
  }

  [NotNull]
  private Task DiscordSocketClientOnJoinedGuild([NotNull] SocketGuild arg)
  {
    _logger.LogDebug("{Username} has joined Guild {GuildTag}", LoggedInUsername, DiscordHelper.GetGuildTag(arg));

    return Task.CompletedTask;
  }

  private async Task DiscordSocketClientOnUserVoiceStateUpdated([NotNull] SocketUser user, SocketVoiceState oldVoiceState,
    SocketVoiceState newVoiceState)
  {
    // Don't care about bot voice state
    if (user.IsBot && user.Id == _discordSocketClient.CurrentUser.Id) return;

    var guild = (oldVoiceState.VoiceChannel ?? newVoiceState.VoiceChannel).Guild;
    if (guild is null) return;

    var player = await _audioService.Players.GetPlayerAsync(guild.Id);
    if (player is null) return;

    var voiceChannelUsers = await player.DiscordClient.GetChannelUsersAsync(guild.Id, player.VoiceChannelId);
    if (voiceChannelUsers.IsDefaultOrEmpty) return;

    // If the bot is the last user in the voice channel
    if (!voiceChannelUsers.Any(x => _discordSocketClient.CurrentUser.Id != x))
    {
      await _voiceService.InitiateDisconnectLogicAsync(player, TimeSpan.FromSeconds(30));
    }
  }

  #endregion Discord Client Events
}
