using Discord;
using Discord.Interactions;
using Howbot.Core.Attributes;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Lavalink4NET;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Integrations.LyricsJava.Extensions;
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
    string searchRequest)
  {
    try
    {
      await DeferAsync();

      var player =
        await musicService.GetPlayerByContextAsync(Context, true);

      if (player is not null)
      {
        // Get the user, voice state, and channel
        // This should never be null (because of the preconditions)
        var user = Context.User as IGuildUser ?? throw new InvalidOperationException("User is not a guild user.");
        var voiceState = Context.User as IVoiceState ??
                         throw new InvalidOperationException("User is not connected to a voice channel.");
        var channel = Context.Channel as ITextChannel ??
                      throw new InvalidOperationException("Channel is not a text channel.");

        var tracksBeforePlay = player.Queue.Count;

        var response =
          await musicService.PlayTrackBySearchTypeAsync(player, searchRequest, user, voiceState,
            channel);

        if (!response.IsSuccessful)
        {
          await DeleteOriginalResponseAsync();

          ModuleHelper.HandleCommandFailed(response);

          return;
        }

        if (player.Queue.Count > tracksBeforePlay + 1)
        {
          await FollowupAsync($"{Emojis.MusicalNote} Added multiple tracks to the queue.");
          return;
        }

        // Should only happen if the command response doesn't contain the Lavalink track
        if (response.LavalinkTrack is null)
        {
          await FollowupAsync($"{Emojis.MusicalNote} Added track to the queue.");
          return;
        }

        var embed = embedService.CreateTrackAddedToQueueEmbed(new ExtendedLavalinkTrack(response.LavalinkTrack),
          user);

        await FollowupAsync(embed: embed as Embed);
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
          preconditions: [PlayerPrecondition.NotPaused]);

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
        .GetPlayerByContextAsync(Context, preconditions: [PlayerPrecondition.Paused]);

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
          properties.Content =
            $"Now playing {commandResponse.LavalinkTrack?.Title ?? Context.Interaction.Data.ToString()}";
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

      var text = lyrics.Text;
      var startIndex = 0;
      var title = $"Lyrics for {track.Title}";

      // Loop that sends potentially multiple embeds
      while (startIndex < text.Length)
      {
        var embedBuilder = new EmbedBuilder { Title = title, Color = Constants.ThemeColor };

        var descriptionChars = Math.Min(Constants.MaximumEmbedDescriptionLength, text.Length - startIndex);
        var description = text.Substring(startIndex, descriptionChars);
        startIndex += descriptionChars;

        // If there's more text, we have to add it as a field
        if (startIndex < text.Length)
        {
          var fieldChars = Math.Min(Constants.MaximumFieldLength, text.Length - startIndex);
          var field = text.Substring(startIndex, fieldChars);
          startIndex += fieldChars;
          embedBuilder.Fields = [new EmbedFieldBuilder { Name = "...continuation", Value = field }];
        }

        // Removing title from other pages of text
        title = string.Empty;
        embedBuilder.Description = description;

        await FollowupAsync("", embed: embedBuilder.Build());
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
        preconditions: [PlayerPrecondition.QueueNotEmpty]);

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

  [SlashCommand(ClearCommandName, ClearCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task ClearQueueCommandAsync()
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

      var tracksRemoved = await player.Queue.ClearAsync();

      await FollowupAsync($"{Emojis.TrashCan} Removed {tracksRemoved} tracks from the queue.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(ClearQueueCommandAsync));
      throw;
    }
  }

  [SlashCommand(AutoPlayCommandName, AutoPlayCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  public async Task AutoPlayCommandAsync()
  {
    try
    {
      await DeferAsync();

      var player = await musicService.GetPlayerByContextAsync(Context);
      if (player is null)
      {
        return;
      }

      player.IsAutoPlayEnabled = !player.IsAutoPlayEnabled;

      await FollowupAsync($"Autoplay is now {(player.IsAutoPlayEnabled ? "enabled" : "disabled")}.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(AutoPlayCommandAsync));
      throw;
    }
  }
}
