using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Entities;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Preconditions;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;
using static Howbot.Core.Models.Constants.Commands;
using static Howbot.Core.Models.Messages.Responses;
using static Howbot.Core.Models.Permissions.Bot;
using static Howbot.Core.Models.Permissions.User;

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
      _logger.LogError(exception, nameof(JoinCommandAsync));
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
    SearchType searchType = SearchType.YouTube)
  {
    try
    {
      await DeferAsync();

      if (string.IsNullOrEmpty(searchRequest))
      {
        await RespondAsync("You must enter a search request!", ephemeral: true);
        return;
      }

      var user = Context.User as IGuildUser;
      var voiceState = Context.User as IVoiceState;
      var channel = Context.Channel as ITextChannel;

      var commandResponse =
        await _musicService.PlayBySearchTypeAsync(searchType, searchRequest, user, voiceState, channel);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(PlayCommandAsync));
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
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = BotTrackPaused);
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(PauseCommandAsync));
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
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "Resuming track");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(ResumeCommandAsync));
      throw;
    }
  }

  [SlashCommand(SeekCommandName, SeekCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task SeekCommandAsync(int hours = 0, int minutes = 0, int seconds = 0, TimeSpan timeToSeek = new())
  {
    try
    {
      await DeferAsync();

      if (hours == 0 && minutes == 0 && seconds == 0 && timeToSeek == default)
      {
        await ModifyOriginalResponseAsync(
          properties => properties.Content = "You have entered an invalid time to seek.");

        throw new ArgumentNullException(nameof(timeToSeek));
      }

      CommandResponse commandResponse;

      if (timeToSeek != default)
      {
        commandResponse = await _musicService.SeekTrackAsync(Context.Guild, timeToSeek);
      }
      else
      {
        timeToSeek = new TimeSpan(hours, minutes, seconds);
        commandResponse = await _musicService.SeekTrackAsync(Context.Guild, timeToSeek);
      }

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = $"Seeking to {timeToSeek:g}");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(SeekCommandAsync));
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
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "Skipping to spot in queue");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(SkipCommandAsync));
      throw;
    }
  }

  [SlashCommand(VolumeCommandName, VolumeCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task VolumeCommandAsync(int newVolume = 0)
  {
    try
    {
      await DeferAsync();

      var commandResponse = await _musicService.ChangeVolumeAsync(Context.Guild, newVolume);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = $"Changing volume to {newVolume}");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(VolumeCommandAsync));
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
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        if (commandResponse.Embed != null)
        {
          await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
        }
        else
        {
          await ModifyOriginalResponseAsync(properties => properties.Content = "Now playing your song!");
        }
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(NowPlayingCommandAsync));
      throw;
    }
  }

  [SlashCommand(GeniusCommandName, GeniusCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  [RequireOwner]
  public async Task GetLyricsFromGeniusAsync()
  {
    try
    {
      await DeferAsync(true);

      var commandResponse = await _musicService.GetLyricsFromGeniusAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

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
      _logger.LogError(exception, nameof(GetLyricsFromGeniusAsync));
      throw;
    }
  }

  [SlashCommand(OvhCommandName, OvhCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  [RequireOwner]
  public async Task GetLyricsFromOvhAsync()
  {
    try
    {
      await DeferAsync(true);

      var commandResponse = await _musicService.GetLyricsFromOvhAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

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
      _logger.LogError(exception, nameof(GetLyricsFromOvhAsync));
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
    try
    {
      await DeferAsync();

      var commandResponse = await _voiceService.LeaveVoiceChannelAsync(Context.User as IGuildUser);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse, _logger);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(LeaveVoiceChannelCommandAsync));
      throw;
    }
  }

  [SlashCommand("radio", "Plays songs from a radio station by a given genre", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  [RequireOwner]
  public async Task RadioCommandAsync()
  {
    try
    {
      if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
      {
        _logger.LogError("Unable to get lava player for voice channel.");
        await RespondAsync(NoPlayerInVoiceChannelResponse);
        return;
      }

      _logger.LogDebug("Playing radio");

      await RespondAsync("Playing radio");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(RadioCommandAsync));
      throw;
    }
  }
  
  [SlashCommand("shuffle", "Shuffle the queue", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task ShuffleCommandAsync()
  {
    try
    {
      if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
      {
        _logger.LogError("Unable to get lava player for voice channel.");
        await RespondAsync(NoPlayerInVoiceChannelResponse);
        return;
      }

      _logger.LogDebug("Shuffling all of the songs in the queue.");
      player.Vueue.Shuffle();

      await RespondAsync("Shuffled the queue.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(ShuffleCommandAsync));
      throw;
    }
  }
  
  [SlashCommand("247", "Toggle 24/7 mode", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task TwentyFourSevenCommandAsync()
  {
    try
    {
      if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
      {
        _logger.LogError("Unable to get lava player for voice channel.");
        await RespondAsync(NoPlayerInVoiceChannelResponse);
        return;
      }

      _logger.LogDebug(player.Is247ModeEnabled ? "Turning off 24/7 mode." : "Turning on 24/7 mode.");
      
      await RespondAsync(player.Is247ModeEnabled ? "Turning off 24/7 mode." : "Turning on 24/7 mode.");
      
      player.Toggle247Mode();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(TwentyFourSevenCommandAsync));
      throw;
    }
  }

  #endregion
}
