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
using static Howbot.Core.Models.Messages.Debug;
using static Howbot.Core.Models.Messages.Errors;
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
        await ModifyOriginalResponseAsync(properties => properties.Content = "You must enter a search request!");
        return;
      }

      var user = Context.User as IGuildUser;
      var voiceState = Context.User as IVoiceState;
      var channel = Context.Channel as ITextChannel;

      CommandResponse commandResponse =
        await _musicService.PlayBySearchTypeAsync(searchType, searchRequest, user, voiceState, channel);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);
      }
      
      // Because embeds are handled by events, just delete the deferred response
      await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
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
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }
      
      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
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
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }
      
      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
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

      if (!ModuleHelper.CheckValidCommandParameter(hours, minutes, seconds, timeToSeek))
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = BotInvalidTimeArgs);
        throw new ArgumentNullException(nameof(timeToSeek));
      }

      TimeSpan timeSpan =
        (timeToSeek == default) ? ModuleHelper.ConvertToTimeSpan(hours, minutes, seconds) : timeToSeek;

      CommandResponse commandResponse = await _musicService.SeekTrackAsync(Context.Guild, timeSpan);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }
      
      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
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
  public async Task SkipCommandAsync(int tracksToSkip = 0)
  {
    try
    {
      await DeferAsync();

      var commandResponse = await _musicService.SkipTrackAsync(Context.Guild, tracksToSkip);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }
      
      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
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
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }
      
      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
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
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }
      if (commandResponse.Embed != null)
      {
        await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = $"Now playing {(commandResponse.LavaPlayer?.Track.Title ?? this.Context.Interaction.Data.ToString())}");
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
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }

      if (commandResponse.Embed != null)
      {
        await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
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
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }

      if (commandResponse.Embed != null)
      {
        await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(GetLyricsFromOvhAsync));
      throw;
    }
  }

  [SlashCommand(RadioCommandName, RadioCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  [RequireOwner]
  public async Task RadioCommandAsync()
  {
    try
    {
      _logger.LogDebug(Messages.Debug.PlayingRadio);
      await RespondAsync("This command is not quite ready yet. Check back later.");

      // await RespondAsync(Messages.Responses.PlayingRadio);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(RadioCommandAsync));
      throw;
    }
  }
  
  [SlashCommand(ShuffleCommandName, ShuffleCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task ShuffleCommandAsync()
  {
    try
    {
      await DeferAsync();
      
      CommandResponse commandResponse = _musicService.ShuffleQueue(Context.Guild);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }
      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(ShuffleCommandAsync));
      throw;
    }
  }
  
  [SlashCommand(TwoFourSevenCommandName, TwoFourSevenCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task TwentyFourSevenCommandAsync()
  {
    try
    {
      await DeferAsync();

      CommandResponse commandResponse = _musicService.ToggleTwoFourSeven(Context.Guild);

      if (!commandResponse.Success)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(async task => await task.Result.DeleteAsync());
          return;
        }
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(TwentyFourSevenCommandAsync));
      throw;
    }
  }

  #endregion
}
