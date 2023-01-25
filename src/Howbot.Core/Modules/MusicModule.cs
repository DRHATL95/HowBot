using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Responses.Search;

namespace Howbot.Core.Modules;

public class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IVoiceService _voiceService;
  private readonly IMusicService _musicService;
  private readonly ILoggerAdapter<MusicModule> _logger;

  public MusicModule(IVoiceService voiceService, IMusicService musicService, ILoggerAdapter<MusicModule> logger)
  {
    _voiceService = voiceService;
    _musicService = musicService;
    _logger = logger;
  }

  [SlashCommand(Constants.Commands.JoinCommandName, Constants.Commands.JoinCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoiceCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoiceCommandPermission)]
  public async Task JoinCommandAsync()
  {
    try
    {
      await this.DeferAsync();
      
      var commandResponse = await _voiceService.JoinVoiceAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);
      
      if (!commandResponse.Success)
      {
        if (commandResponse.Exception != null) throw commandResponse.Exception;

        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

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

  [SlashCommand(Constants.Commands.PlayCommandName, Constants.Commands.PlayCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoicePlayCommandPermission)]
  public async Task PlayCommandAsync(
    [Summary(Constants.Commands.PlaySearchRequestArgumentName, Constants.Commands.PlaySearchRequestArgumentDescription)] string searchRequest,
    [Summary(Constants.Commands.PlaySearchTypeArgumentName, Constants.Commands.PlaySearchTypeArgumentDescription)] SearchType? searchType = null)
  {
    try
    {
      await DeferAsync(true);

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
        commandResponse = await _musicService.PlayBySearchTypeAsync(SearchType.YouTube, searchRequest, user, voiceState, channel);
      }
      else
      {
        commandResponse = await _musicService.PlayBySearchTypeAsync(searchType.Value, searchRequest, user, voiceState, channel);
      }

      if (!commandResponse.Success)
      {
        if (commandResponse.Exception != null) throw commandResponse.Exception;

        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogInformation(commandResponse.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        _logger.LogDebug("Else");
        // Should create embed somewhere after this point
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown playing song");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.PauseCommandName, Constants.Commands.PauseCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoicePlayCommandPermission)]
  public async Task PauseCommandAsync()
  {
    try
    {
      await DeferAsync(ephemeral: true);
      
      var commandResponse = await _musicService.PauseTrackAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(PauseCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = Messages.Responses.BotTrackPaused);
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Error has been thrown");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.ResumeCommandName, Constants.Commands.ResumeCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoicePlayCommandPermission)]
  public async Task ResumeCommandAsync()
  {
    try
    {
      await DeferAsync(ephemeral: true);
      
      var commandResponse = await _musicService.ResumeTrackAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(ResumeCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

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

  [SlashCommand("seek", "Seek something", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoicePlayCommandPermission)]
  public async Task SeekCommandAsync(TimeSpan timeToSeek)
  {
    try
    {
      await DeferAsync(ephemeral: true);
      
      var commandResponse = await _musicService.SeekTrackAsync(Context.Guild, timeToSeek);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(SeekCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

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

  [SlashCommand("skip", "skip either to next track or a valid number of tracks in the queue", true, RunMode.Async)]
  public async Task SkipCommandAsync(int? tracksToSkip = null)
  {
    try
    {
      await DeferAsync(ephemeral: true);
      
      var commandResponse = await _musicService.SkipTrackAsync(Context.Guild, tracksToSkip ?? 0);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(SkipCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

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

  [SlashCommand("volume", "Change volume", true, RunMode.Async)]
  public async Task VolumeCommandAsync(int? newVolume = 0)
  {
    try
    {
      await DeferAsync(ephemeral: true);
      
      var commandResponse = await _musicService.ChangeVolumeAsync(Context.Guild, newVolume ?? 0);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(VolumeCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

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

  [SlashCommand("np", "Gets an embed of the current playing track", true, RunMode.Async)]
  public async Task NowPlayingCommandAsync()
  {
    try
    {
      await DeferAsync(ephemeral: true);
      
      var commandResponse = await _musicService.NowPlayingAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(NowPlayingCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

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

  [SlashCommand("genius", "Gets lyrics for current playing track from Genius", true, RunMode.Async)]
  public async Task GetLyricsFromGeniusAsync()
  {
    try
    {
      await DeferAsync(ephemeral: true);
      
      var commandResponse = await _musicService.GetLyricsFromGeniusAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(GetLyricsFromGeniusAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
      
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

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

  [SlashCommand("ovo", "Gets lyrics for current playing track from Ovh lyrics")]
  public async Task GetLyricsFromOvhAsync()
  {
    try
    {
      await DeferAsync(ephemeral: true);
      
      var commandResponse = await _musicService.GetLyricsFromOvhAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(GetLyricsFromGeniusAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
      
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

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
}
