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
using Lavalink4NET;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Integrations.LyricsJava.Extensions;
using Lavalink4NET.Lyrics;
using Lavalink4NET.Players.Preconditions;
using static Howbot.Core.Models.Constants.Commands;
using static Howbot.Core.Models.Messages.Responses;
using static Howbot.Core.Models.Permissions.Bot;
using static Howbot.Core.Models.Permissions.User;

namespace Howbot.Core.Modules;

public class MusicModule(
  IMusicService musicService,
  IEmbedService embedService,
  IAudioService audioService,
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

        int tracksBeforePlay = player.Queue.Count;

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
          // Added more than one track to the queue
          if (player.Queue.Count > (tracksBeforePlay + 1))
          {
            await FollowupAsync("🎵 Added multiple tracks to the queue.");
          }
          else
          {
            if (response.LavalinkTrack is null)
            {
              await FollowupAsync("🎵 Added track to the queue.");
            }
            else
            {
              var embed = embedService.CreateTrackAddedToQueueEmbed(new ExtendedLavalinkTrack(response.LavalinkTrack),
                               user);

              await FollowupAsync(embed: embed as Embed);
            }
          }
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

      var timeSpan =
        timeToSeek == default ? ModuleHelper.ConvertToTimeSpan(hours, minutes, seconds) : timeToSeek;

      var player = await musicService.GetPlayerByContextAsync(Context, false, true, [PlayerPrecondition.Playing]);

      if (player is null)
      {
        return;
      }
      
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

      var player = await musicService.GetPlayerByContextAsync(Context, false, true, [PlayerPrecondition.Playing]);

      if (player is null)
      {
        return;
      }

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
  public async Task VolumeCommandAsync(int? newVolume = null)
  {
    try
    {
      await DeferAsync();

      var player = await musicService.GetPlayerByContextAsync(Context, false, true, [PlayerPrecondition.Playing]);

      if (player is null)
      {
        return;
      }

      if (!newVolume.HasValue)
      {
        // Respond with the current volume
        await ModifyOriginalResponseAsync(properties =>
          properties.Content = $"🔊 Current volume is {player.Volume * 100}%");

        return;
      }

      var commandResponse = await musicService.ChangeVolumeAsync(player, newVolume.Value);

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

  [SlashCommand(NowPlayingCommandName, NowPlayingCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task NowPlayingCommandAsync()
  {
    try
    {
      await DeferAsync();

      var player = await musicService.GetPlayerByContextAsync(Context, preconditions: [PlayerPrecondition.Playing]);

      if (player is null)
      {
        return;
      }


      if (Context.User is not IGuildUser user || Context.Channel is not ITextChannel textChannel)
      {
        return;
      }

      var commandResponse = musicService.NowPlaying(player, user, textChannel);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await DeleteOriginalResponseAsync();
          return;
        }
      }

      await ModifyOriginalResponseAsync(properties =>
      {
        if (commandResponse.Embed != null)
        {
          properties.Embed = commandResponse.Embed as Embed;
        }
        else
        {
          properties.Content = $"Now playing {commandResponse.LavalinkTrack?.Title ?? Context.Interaction.Data.ToString()}";
        }
      });
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(NowPlayingCommandAsync));
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
        preconditions: [PlayerPrecondition.QueueNotEmpty]);

      if (player is null)
      {
        return;
      }

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
  [RequireGuildUserInVoiceChannel]
  public async Task LyricsCommandAsync()
  {
    try
    {
      await DeferAsync();

      var player = await musicService.GetPlayerByContextAsync(Context,
        preconditions: [PlayerPrecondition.Playing]);

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

      var lyrics = await audioService.Tracks.GetCurrentTrackLyricsAsync(player);

      if (lyrics is null)
      {
        await FollowupAsync("😖 No lyrics found.");
        return;
      }

      // Check if lyrics length is less or equal to 2000 characters
      if (lyrics.Text.Length <= 2000)
      {
        await FollowupAsync($"{lyrics.Text}");
      }
      else
      {
        for (int i = 0; i < lyrics.Text.Length; i += 2000)
        {
          await DeleteOriginalResponseAsync();

          await Context.Channel.SendMessageAsync(lyrics.Text.Substring(i, Math.Min(2000, lyrics.Text.Length - i)));
        }
      }
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
        preconditions: [PlayerPrecondition.Playing, PlayerPrecondition.QueueNotEmpty, PlayerPrecondition.Paused]);

      if (player is null)
      {
        return;
      }

      if (!player.Queue.Any())
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "No tracks in queue.");
        return;
      }

      var commandResponse = musicService.GetMusicQueueForServer(player);

      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await DeleteOriginalResponseAsync();
          return;
        }
      }

      await ModifyOriginalResponseAsync(properties =>
      {
        if (commandResponse.Embed != null)
        {
          properties.Embed = commandResponse.Embed as Embed;
        }
        else
        {
          // TODO: When the queue is empty, this is throwing an error. Create an embed to display "No tracks in queue."
          properties.Content = player.Queue.Select(x => x.Track?.Title).Aggregate((x, y) => $"{x}\n{y}");
        }
      });
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(GetQueueCommandAsync));
      throw;
    }
  }
}
