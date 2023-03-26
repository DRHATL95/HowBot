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
using Howbot.Core.Modules;
using Howbot.Core.Settings;
using Microsoft.Extensions.Logging;
using Victoria.Node;
using Victoria.Player;

namespace Howbot.Core.Services;

public class DiscordClientService : ServiceBase<DiscordClientService>, IDiscordClientService
{
  private readonly DiscordSocketClient _discordSocketClient;
  private readonly InteractionService _interactionService;
  private readonly ILavaNodeService _lavaNodeService;
  private readonly LavaNode<Player<LavaTrack>, LavaTrack> _lavaNode;
  private readonly ILoggerAdapter<DiscordClientService> _logger;
  private readonly IServiceProvider _serviceProvider;

  public DiscordClientService(DiscordSocketClient discordSocketClient, ILavaNodeService lavaNodeService, IServiceProvider serviceProvider,
    InteractionService interactionService, LavaNode<Player<LavaTrack>, LavaTrack> lavaNode,
    ILoggerAdapter<DiscordClientService> logger) : base(logger)
  {
    _discordSocketClient = discordSocketClient;
    _serviceProvider = serviceProvider;
    _lavaNodeService = lavaNodeService;
    _interactionService = interactionService;
    _lavaNode = lavaNode;
    _logger = logger;
  }

  public new void Initialize()
  {
    if (_discordSocketClient == null) return;

    if (_logger.IsLogLevelEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("{ServiceName} is initializing..", nameof(DiscordClientService));
    }

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
    if (string.IsNullOrEmpty(discordToken)) throw new ArgumentNullException(nameof(discordToken));

    try
    {
      await _discordSocketClient.LoginAsync(TokenType.Bot, discordToken);

      return true;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Unable to login to discord API with token");
      return false;
    }
  }

  public async ValueTask<bool> StartDiscordBotAsync()
  {
    try
    {
      // Wait until bot has fully logged into Discord.API. Will run in separate thread to not slow down main thread.
      await Task.Run(() =>
      {
        while (_discordSocketClient.LoginState != LoginState.LoggedIn)
          // Only check after 3 seconds.
          Thread.Sleep(3000);
      });

      // Will signal ready state. Must be called only when bot has finished logging in.
      await _discordSocketClient.StartAsync();

      // Only in debug, set bots online presence to offline
      if (Configuration.IsDebug())
      {
        await _discordSocketClient.SetStatusAsync(UserStatus.Invisible);
      }

      // Add modules dynamically to discord bot
      await this.AddModulesToDiscordBotAsync();

      return true;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Unable to start discord bot");
      throw;
    }
  }

  private async Task AddModulesToDiscordBotAsync()
  {
    try
    {
      await _interactionService.AddModuleAsync(typeof(MusicModule), _serviceProvider);
      await _interactionService.AddModuleAsync(typeof(GeneralModule), _serviceProvider);
    }
    catch (FileNotFoundException exception)
    {
      _logger.LogError(exception, "Unable to find the assembly. Value: {AssemblyName}",
        Assembly.GetEntryAssembly()?.ToString());
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

    _logger.Log(severity, arg.Message);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnUserJoined(SocketGuildUser arg)
  {
    _logger.LogDebug("{GuildUserName} has joined Guild {GuildTag}", arg.Username, GuildHelper.GetGuildTag(arg.Guild));

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnSlashCommandExecuted(SocketSlashCommand arg)
  {
    var guild = _discordSocketClient.Guilds.FirstOrDefault(x => x.Id == arg.GuildId);
    if (guild == null)
    {
      _logger.LogError(new Exception(), "Unable to look-up guild for event [{EventName}]",
        nameof(DiscordSocketClientOnSlashCommandExecuted));
    }

    _logger.LogDebug("Command [{CommandName}] has been executed in Guild {GuildTag}", arg.CommandName,
      GuildHelper.GetGuildTag(guild));

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnDisconnected(Exception arg)
  {
    _logger.LogDebug("{Username} has disconnected from socket", Constants.BotName);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnConnected()
  {
    _logger.LogDebug("{Username} has connected to socket", Constants.BotName);

    return Task.CompletedTask;
  }

  private async Task DiscordSocketClientOnReady()
  {
    _logger.LogDebug("{Username} is now in READY state", Constants.BotName);

    try
    {
      if (Configuration.IsDebug())
      {
        _logger.LogDebug("Registering commands to DEV Guild");
        await _interactionService.RegisterCommandsToGuildAsync(Constants.DiscordDevelopmentGuildId);
      }
      else
      {
        _logger.LogDebug("Registering commands globally");
        await _interactionService.RegisterCommandsGloballyAsync();
      }

      _logger.LogDebug("Successfully registered commands to discord bot");

      if (!_lavaNode.IsConnected)
      {
        _logger.LogDebug("Connecting to lavalink server");

        await _lavaNode.ConnectAsync();

        _logger.LogDebug("Successfully connected to lavalink server");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in client ready event");
      throw;
    }
  }

  private Task DiscordSocketClientOnLoggedOut()
  {
    _logger.LogDebug("{Username} has logged out successfully", Constants.BotName);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnLoggedIn()
  {
    _logger.LogDebug("{Username} has logged in successfully", Constants.BotName);

    return Task.CompletedTask;
  }

  private Task DiscordSocketClientOnJoinedGuild(SocketGuild arg)
  {
    _logger.LogDebug("{Username} has joined Guild {GuildTag}", Constants.BotName, GuildHelper.GetGuildTag(arg));

    return Task.CompletedTask;
  }
  
  private async Task DiscordSocketClientOnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldVoiceState,
    SocketVoiceState newVoiceState)
  {
    // Don't care about bot voice state
    if (user.IsBot && user.Id == _discordSocketClient.CurrentUser.Id) return;

    if (!_lavaNode.TryGetPlayer(oldVoiceState.VoiceChannel.Guild, out var player)) return;
    
    // Get the voice channel the bot is in
    var voiceChannel = _discordSocketClient.Guilds
      .Select(g => g.VoiceChannels.FirstOrDefault(vc => vc.Users.Any(u => u.Id == _discordSocketClient.CurrentUser.Id)))
      .FirstOrDefault();

    if (voiceChannel != null && !voiceChannel.Users.Any(x => x.Id != _discordSocketClient.CurrentUser.Id && x.VoiceChannel != null))
    {
      await Task.Run(() =>
      {
          // Leave the voice channel automatically when no one else is in voice
          _lavaNodeService.InitiateDisconnectLogicAsync(player, TimeSpan.FromSeconds(30));
      });
    }
  }
  
  #endregion
}
