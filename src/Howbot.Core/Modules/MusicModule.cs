using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET.Lyrics;
using Lavalink4NET.Players.Preconditions;
using Lavalink4NET.Players.Queued;
using static Howbot.Core.Models.Constants.Commands;
using static Howbot.Core.Models.Messages.Responses;
using static Howbot.Core.Models.Permissions.Bot;
using static Howbot.Core.Models.Permissions.User;

namespace Howbot.Core.Modules;

public class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
  [NotNull] private readonly ILoggerAdapter<MusicModule> _logger;
  [NotNull] private readonly IMusicService _musicService;
  [NotNull] private readonly ILyricsService _lyricsService;

  public MusicModule(IMusicService musicService, ILyricsService lyricsService, ILoggerAdapter<MusicModule> logger)
  {
    _musicService = musicService;
    _lyricsService = lyricsService;
    _logger = logger;
  }

  [SlashCommand(PlayCommandName, PlayCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task PlayCommandAsync(
    [Summary(PlaySearchRequestArgumentName, PlaySearchRequestArgumentDescription)] [NotNull]
    string searchRequest,
    [Summary(PlaySearchTypeArgumentName, PlaySearchTypeArgumentDescription)]
    SearchProviderTypes searchProviderType)
  {
    try
    {
      await DeferAsync().ConfigureAwait(false);

      if (string.IsNullOrEmpty(searchRequest))
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "You must enter a search request!").ConfigureAwait(false);
        return;
      }

      var player =
        await _musicService.GetPlayerByContextAsync(Context, true).ConfigureAwait(false);

      if (player is not null)
      {
        var user = (Context.User as IGuildUser) ?? throw new ArgumentNullException(nameof(Context.User));
        var voiceState = (Context.User as IVoiceState) ?? throw new ArgumentNullException(nameof(Context.User));
        var channel = (Context.Channel as ITextChannel) ?? throw new ArgumentNullException(nameof(Context.Channel));

        var response =
          await _musicService.PlayTrackBySearchTypeAsync(player, searchProviderType, searchRequest, user, voiceState,
            channel).ConfigureAwait(false);

        if (!response.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(response);
          // Because embeds are handled by events, just delete the deferred response
          await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
          return;
        }

        if (player.Queue.Count == 0)
        {
          await FollowupAsync($"🔈 Playing: <{response.LavalinkTrack?.Uri}>").ConfigureAwait(false);
        }
        else
        {
          await FollowupAsync($"Adding <{response.LavalinkTrack?.Uri}> to the server queue.");
        }
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

      var player =
        await _musicService.GetPlayerByContextAsync(Context,
          preconditions: ImmutableArray.Create(PlayerPrecondition.NotPaused)).ConfigureAwait(false);

      if (player is not null)
      {
        var response = await _musicService.PauseTrackAsync(player);

        if (!response.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(response);

          if (string.IsNullOrEmpty(response.Message))
          {
            await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
            return;
          }

          await ModifyOriginalResponseAsync(properties => properties.Content = response.Message);
        }
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

      var player = await _musicService.GetPlayerByContextAsync(Context, preconditions: ImmutableArray.Create(PlayerPrecondition.Paused)).ConfigureAwait(false);

      if (player is not null)
      {
        var commandResponse = await _musicService.ResumeTrackAsync(player);

        if (!commandResponse.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(commandResponse);

          if (string.IsNullOrEmpty(commandResponse.Message))
          {
            await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
            return;
          }
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
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

      if (!ModuleHelper.CheckValidCommandParameter(hours, minutes, seconds, timeToSeek))
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = BotInvalidTimeArgs);
        throw new ArgumentNullException(nameof(timeToSeek));
      }

      var player = await _musicService.GetPlayerByContextAsync(Context).ConfigureAwait(false);


      TimeSpan timeSpan =
        (timeToSeek == default) ? ModuleHelper.ConvertToTimeSpan(hours, minutes, seconds) : timeToSeek;

      CommandResponse commandResponse = await _musicService.SeekTrackAsync<IQueuedLavalinkPlayer>(player, timeSpan);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(([NotNull] task) => task.Result.DeleteAsync().ConfigureAwait(false));
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
  public async Task SkipCommandAsync(int tracksToSkip = 1)
  {
    try
    {
      await DeferAsync();

      var player = await _musicService.GetPlayerByContextAsync(Context);

      var commandResponse = await _musicService.SkipTrackAsync(player, tracksToSkip);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
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

      var player = await _musicService.GetPlayerByContextAsync(Context).ConfigureAwait(false);

      var commandResponse = await _musicService.ChangeVolumeAsync(player, newVolume);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
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

      var player = await _musicService.GetPlayerByContextAsync(Context, preconditions: ImmutableArray.Create(PlayerPrecondition.Playing)).ConfigureAwait(false);

      if (Context.User is not IGuildUser guildUser || Context.Channel is not ITextChannel textChannel) return;

      var commandResponse =
        await _musicService.NowPlayingAsync(player, guildUser, textChannel);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
          return;
        }
      }
      if (commandResponse.Embed != null)
      {
        await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = $"Now playing {commandResponse.LavalinkTrack?.Title ?? Context.Interaction.Data.ToString()}");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(NowPlayingCommandAsync));
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

      var player = await _musicService.GetPlayerByContextAsync(Context, preconditions: ImmutableArray.Create(PlayerPrecondition.QueueNotEmpty)).ConfigureAwait(false);

      CommandResponse commandResponse = _musicService.ToggleShuffle(player);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await GetOriginalResponseAsync().ContinueWith(task => task.Result.DeleteAsync().ConfigureAwait(false));
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

  [SlashCommand("lyrics", description: "Searches for lyrics", runMode: RunMode.Async)]
  [RequireOwner]
  public async Task LyricsCommandAsync()
  {
    await DeferAsync().ConfigureAwait(false);

    var player = await _musicService.GetPlayerByContextAsync(Context,
      preconditions: ImmutableArray.Create(PlayerPrecondition.Playing)).ConfigureAwait(false);

    if (player is null)
    {
      return;
    }

    var track = player.CurrentTrack;

    if (track is null)
    {
      await FollowupAsync("🤔 No track is currently playing.").ConfigureAwait(false);
      return;
    }

    var lyrics = await _lyricsService.GetLyricsAsync(track.Title, track.Author).ConfigureAwait(false);

    if (lyrics is null)
    {
      await FollowupAsync("😖 No lyrics found.").ConfigureAwait(false);
      return;
    }

    await FollowupAsync($"📃 Lyrics for {track.Title} by {track.Author}:\n{lyrics}").ConfigureAwait(false);
  }

  /*[SlashCommand(TwoFourSevenCommandName, TwoFourSevenCommandDescription, true, RunMode.Async)]
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
  }*/

}
