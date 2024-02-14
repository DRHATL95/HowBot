using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Lyrics;
using Lavalink4NET.Players.Preconditions;
using static Howbot.Core.Models.Constants.Commands;
using static Howbot.Core.Models.Messages.Responses;
using static Howbot.Core.Models.Permissions.Bot;
using static Howbot.Core.Models.Permissions.User;

namespace Howbot.Core.Modules;

public class MusicModule(
  IMusicService musicService,
  ILyricsService lyricsService,
  IEmbedService embedService,
  ILoggerAdapter<MusicModule> logger)
  : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand(PlayCommandName, PlayCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task PlayCommandAsync(
    [Summary(PlaySearchRequestArgumentName, PlaySearchRequestArgumentDescription)]
    string searchRequest,
    [Summary(PlaySearchTypeArgumentName, PlaySearchTypeArgumentDescription)]
    SearchProviderTypes searchProviderType = SearchProviderTypes.YouTubeMusic)
  {
    try
    {
      await DeferAsync();

      if (string.IsNullOrEmpty(searchRequest))
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "You must enter a search request!");
        return;
      }

      var player =
        await musicService.GetPlayerByContextAsync(Context, true);

      if (player is not null)
      {
        var user = Context.User as IGuildUser ?? throw new ArgumentNullException(nameof(Context.User));
        var voiceState = Context.User as IVoiceState ?? throw new ArgumentNullException(nameof(Context.User));
        var channel = Context.Channel as ITextChannel ?? throw new ArgumentNullException(nameof(Context.Channel));

        var response =
          await musicService.PlayTrackBySearchTypeAsync(player, searchProviderType, searchRequest, user, voiceState,
            channel);

        if (!response.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(response);
          await DeleteOriginalResponseAsync();
          return;
        }

        if (player.Queue.Any())
        {
          var embed = embedService.CreateTrackAddedToQueueEmbed(new ExtendedLavalinkTrack(response.LavalinkTrack),
            user);

          await FollowupAsync(embed: embed as Embed);
        }
        else
        {
          await DeleteOriginalResponseAsync();
        }
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PlayCommandAsync));
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
        await musicService.GetPlayerByContextAsync(Context,
          preconditions: ImmutableArray.Create(PlayerPrecondition.NotPaused));

      if (player is not null)
      {
        var response = await musicService.PauseTrackAsync(player);

        if (!response.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(response);

          if (string.IsNullOrEmpty(response.Message))
          {
            await DeleteOriginalResponseAsync();
            return;
          }

          await ModifyOriginalResponseAsync(properties => properties.Content = response.Message);
        }
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PauseCommandAsync));
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

      var player = await musicService
        .GetPlayerByContextAsync(Context, preconditions: ImmutableArray.Create(PlayerPrecondition.Paused));

      if (player is not null)
      {
        var commandResponse = await musicService.ResumeTrackAsync(player);

        if (!commandResponse.IsSuccessful)
        {
          ModuleHelper.HandleCommandFailed(commandResponse);

          if (string.IsNullOrEmpty(commandResponse.Message))
          {
            await DeleteOriginalResponseAsync();
            return;
          }
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(ResumeCommandAsync));
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

      var player = await musicService.GetPlayerByContextAsync(Context);

      var timeSpan =
        timeToSeek == default ? ModuleHelper.ConvertToTimeSpan(hours, minutes, seconds) : timeToSeek;

      var commandResponse = await musicService.SeekTrackAsync(player, timeSpan);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await DeleteOriginalResponseAsync();
          return;
        }
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(SeekCommandAsync));
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

      var player = await musicService.GetPlayerByContextAsync(Context);

      var commandResponse = await musicService.SkipTrackAsync(player, tracksToSkip);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await DeleteOriginalResponseAsync();
          return;
        }
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(SkipCommandAsync));
      throw;
    }
  }

  [SlashCommand(VolumeCommandName, VolumeCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task VolumeCommandAsync(int newVolume = 100)
  {
    try
    {
      await DeferAsync();

      var player = await musicService.GetPlayerByContextAsync(Context);

      var commandResponse = await musicService.ChangeVolumeAsync(player, newVolume);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await DeleteOriginalResponseAsync();
          return;
        }
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(VolumeCommandAsync));
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

      var player = await musicService
        .GetPlayerByContextAsync(Context, preconditions: ImmutableArray.Create(PlayerPrecondition.Playing));

      if (Context.User is not IGuildUser guildUser || Context.Channel is not ITextChannel textChannel)
      {
        return;
      }

      var commandResponse =
        musicService.NowPlaying(player, guildUser, textChannel);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await DeleteOriginalResponseAsync();
          return;
        }
      }

      if (commandResponse.Embed != null)
      {
        await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties =>
          properties.Content =
            $"Now playing {commandResponse.LavalinkTrack?.Title ?? Context.Interaction.Data.ToString()}");
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(NowPlayingCommandAsync));
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
      logger.LogDebug(Messages.Debug.PlayingRadio);

      await RespondAsync("This command is not quite ready yet. Check back later.");

      // await RespondAsync(Messages.Responses.PlayingRadio);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(RadioCommandAsync));
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

      var player = await musicService.GetPlayerByContextAsync(Context,
        preconditions: ImmutableArray.Create(PlayerPrecondition.QueueNotEmpty));

      var commandResponse = musicService.ToggleShuffle(player);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await DeleteOriginalResponseAsync();
          return;
        }
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(ShuffleCommandAsync));
      throw;
    }
  }

  [SlashCommand(LyricsCommandName, LyricsCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireOwner]
  public async Task LyricsCommandAsync()
  {
    try
    {
      await DeferAsync();

      var player = await musicService.GetPlayerByContextAsync(Context,
        preconditions: ImmutableArray.Create(PlayerPrecondition.Playing));

      if (player is null)
      {
        return;
      }

      var track = player.CurrentTrack;

      if (track is null)
      {
        await FollowupAsync("🤔 No track is currently playing.");
        return;
      }

      var lyrics = await lyricsService.GetLyricsAsync(track.Title, track.Author);

      if (lyrics is null)
      {
        await FollowupAsync("😖 No lyrics found.");
        return;
      }

      await FollowupAsync($"📃 Lyrics for {track.Title} by {track.Author}:\n{lyrics}");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(LyricsCommandAsync));
      throw;
    }
  }

  [SlashCommand(QueueCommandName, QueueCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task GetQueueCommandAsync()
  {
    try
    {
      await DeferAsync();

      var player = await musicService.GetPlayerByContextAsync(Context,
        preconditions: ImmutableArray.Create(PlayerPrecondition.Playing));

      if (player is null)
      {
        return;
      }

      var commandResponse = musicService.GetGuildMusicQueueEmbed(player);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await DeleteOriginalResponseAsync();
          return;
        }
      }

      if (commandResponse.Embed != null)
      {
        await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
      }
      else
      {
        // TODO: When the queue is empty, this is throwing an error. Create an embed to display "No tracks in queue."
        await ModifyOriginalResponseAsync(properties =>
          properties.Content = player.Queue.Select(x => x.Track?.Title).Aggregate((x, y) => $"{x}\n{y}"));
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(GetQueueCommandAsync));
      throw;
    }
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
