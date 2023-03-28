using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using Howbot.Core.Preconditions;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;
using static Howbot.Core.Constants.Commands;
using static Howbot.Core.Messages.Responses;
using static Howbot.Core.Permissions.Bot;
using static Howbot.Core.Permissions.User;

namespace Howbot.Core.Modules;

public class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly LavaNode<Player<LavaTrack>, LavaTrack> _lavaNode;
  private readonly ILoggerAdapter<MusicModule> _logger;
  private readonly IMusicService _musicService;
  private readonly IVoiceService _voiceService;

  public MusicModule(IVoiceService voiceService, IMusicService musicService,
    LavaNode<Player<LavaTrack>, LavaTrack> lavaNode, ILoggerAdapter<MusicModule> logger)
  {
    _voiceService = voiceService;
    _musicService = musicService;
    _lavaNode = lavaNode;
    _logger = logger;
  }

  #region Music Module Slash Commands

  [SlashCommand(JoinCommandName, JoinCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoiceCommandPermission)]
  [RequireUserPermission(GuildUserVoiceCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task JoinCommandAsync()
  {
    try
    {
      await DeferAsync();

      var commandResponse =
        await _voiceService.JoinVoiceAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);

      if (!commandResponse.Success)
      {
        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogDebug(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = "Command did not run successfully.");
      }
      else
      {
        await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown executing command [{CommandName}]", nameof(JoinCommandAsync));
      throw;
    }
  }

  [SlashCommand(PlayCommandName, PlayCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task PlayCommandAsync(
    [Summary(PlaySearchRequestArgumentName, PlaySearchRequestArgumentDescription)]
    string searchRequest,
    [Summary(PlaySearchTypeArgumentName, PlaySearchTypeArgumentDescription)]
    SearchType? searchType = null)
  {
    try
    {
      await DeferAsync();

      if (string.IsNullOrEmpty(searchRequest))
      {
        await RespondAsync("You must enter a search request!", ephemeral: true);
        return;
      }

      CommandResponse commandResponse;
      var user = Context.User as IGuildUser;
      var voiceState = Context.User as IVoiceState;
      var channel = Context.Channel as ITextChannel;

      if (!searchType.HasValue)
      {
        // Default to YouTube
        commandResponse =
          await _musicService.PlayBySearchTypeAsync(SearchType.YouTube, searchRequest, user, voiceState, channel);
      }
      else
      {
        commandResponse =
          await _musicService.PlayBySearchTypeAsync(searchType.Value, searchRequest, user, voiceState, channel);
      }

      if (!commandResponse.Success)
      {
        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogInformation(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown playing song");
      throw;
    }
  }

  [SlashCommand(PauseCommandName, PauseCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task PauseCommandAsync()
  {
    try
    {
      await DeferAsync();

      var commandResponse = await _musicService.PauseTrackAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(PauseCommandAsync));

        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogDebug(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = BotTrackPaused);
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Error has been thrown");
      throw;
    }
  }

  [SlashCommand(ResumeCommandName, ResumeCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task ResumeCommandAsync()
  {
    try
    {
      await DeferAsync();

      var commandResponse = await _musicService.ResumeTrackAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(ResumeCommandAsync));

        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogDebug(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        // TODO: Embed or ReplyAsync
        await ModifyOriginalResponseAsync(properties => properties.Content = "Resuming track");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand(SeekCommandName, SeekCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task SeekCommandAsync(int hours = 0, int minutes = 0, int seconds = 0, TimeSpan? timeToSeek = null)
  {
    try
    {
      await DeferAsync();

      if (hours == 0 && minutes == 0 && seconds == 0 && timeToSeek == null)
      {
        throw new ArgumentNullException(nameof(timeToSeek));
      }

      CommandResponse commandResponse;

      if (timeToSeek.HasValue)
      {
        commandResponse = await _musicService.SeekTrackAsync(Context.Guild, timeToSeek.Value);
      }
      else
      {
        var timeSpan = new TimeSpan(hours, minutes, seconds);
        commandResponse = await _musicService.SeekTrackAsync(Context.Guild, timeSpan);
      }

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(SeekCommandAsync));

        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogDebug(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        // TODO: Embed or RespondAsync
        await ModifyOriginalResponseAsync(properties => properties.Content = "Seeking to desired location");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand(SkipCommandName, SkipCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task SkipCommandAsync(int? tracksToSkip = null)
  {
    try
    {
      await DeferAsync();

      var commandResponse = await _musicService.SkipTrackAsync(Context.Guild, tracksToSkip ?? 0);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(SkipCommandAsync));

        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogDebug(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "Skipping to spot in queue");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand(VolumeCommandName, VolumeCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task VolumeCommandAsync(int? newVolume = 0)
  {
    try
    {
      await DeferAsync(true);

      var commandResponse = await _musicService.ChangeVolumeAsync(Context.Guild, newVolume ?? 0);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(VolumeCommandAsync));

        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogDebug(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "Changing volume");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand(NowPlayingCommandName, NowPlayingCommandDescription, true,
    RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task NowPlayingCommandAsync()
  {
    try
    {
      await DeferAsync();

      var commandResponse =
        await _musicService.NowPlayingAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(NowPlayingCommandAsync));

        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogDebug(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        if (commandResponse.Embed != null)
        {
          await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
        }
        // await ModifyOriginalResponseAsync(properties => properties.Content = "Skipping to spot in queue");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand(GeniusCommandName, GeniusCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task GetLyricsFromGeniusAsync()
  {
    try
    {
      await DeferAsync(true);

      var commandResponse = await _musicService.GetLyricsFromGeniusAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(GetLyricsFromGeniusAsync));

        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogDebug(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        if (commandResponse.Embed != null)
        {
          await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
        }
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand(OvhCommandName, OvhCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task GetLyricsFromOvhAsync()
  {
    try
    {
      await DeferAsync(true);

      var commandResponse = await _musicService.GetLyricsFromOvhAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(GetLyricsFromGeniusAsync));

        if (commandResponse.Exception != null)
        {
          throw commandResponse.Exception;
        }

        if (!string.IsNullOrEmpty(commandResponse.Message))
        {
          _logger.LogDebug(commandResponse.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        if (commandResponse.Embed != null)
        {
          await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
        }
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand(LeaveCommandName, LeaveCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task LeaveVoiceChannelCommandAsync()
  {
    await DeferAsync();

    var commandResponse = await _voiceService.LeaveVoiceChannelAsync(Context.User as IGuildUser);

    if (!commandResponse.Success)
    {
      _logger.LogCommandFailed(nameof(LeaveVoiceChannelCommandAsync));

      if (commandResponse.Exception != null)
      {
        throw commandResponse.Exception;
      }

      if (!string.IsNullOrEmpty(commandResponse.Message))
      {
        _logger.LogDebug(commandResponse.Message);
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
    }
    else
    {
      await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
    }
  }

  [SlashCommand("radio", "Play songs related to last played song", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task RadioCommandAsync()
  {
    if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
    {
      _logger.LogError("Unable to get lava player for voice channel.");
      await RespondAsync(NoPlayerInVoiceChannelResponse);
      return;
    }

    _logger.LogInformation("Radio mode toggled.");
    player.ToggleRadioMode();

    var response = player.IsRadioMode ? RadioModeEnabled : RadioModeDisabled;

    await RespondAsync(response);
  }

  #endregion
}
