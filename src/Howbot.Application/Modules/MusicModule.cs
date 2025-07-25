using Discord;
using Discord.Interactions;
using Howbot.Application.Attributes;
using Howbot.Application.Constants;
using Howbot.Application.Helpers;
using Howbot.Application.Interfaces.Discord;
using Howbot.Application.Interfaces.Lavalink;
using Howbot.Application.Models.Discord;
using Howbot.SharedKernel;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using static Howbot.Application.Models.Discord.Messages.Responses;
using static Howbot.Application.Models.Discord.Permissions.Bot;
using static Howbot.Application.Models.Discord.Permissions.User;


namespace Howbot.Application.Modules;

public class MusicModule(
  IPlayerFactoryService playerFactoryService,
  IMusicPlaybackService musicPlaybackService,
  IMusicService musicService,
  IEmbedService embedService,
    ILoggerAdapter<MusicModule> logger)
  : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand(CommandMetadata.PlayCommandName, CommandMetadata.PlayCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(GuildUserVoicePlayCommandPermission)]
  [RequireGuildUserInVoiceChannel]
  public async Task PlayCommandAsync(
    [Summary(CommandMetadata.PlaySearchRequestArgumentName, CommandMetadata.PlaySearchRequestArgumentDescription)]
    string searchRequest)
  {
    try
    {
      await DeferAsync();

      var player = await playerFactoryService.GetOrCreatePlayerAsync(Context);
      if (player is null)
      {
        await FollowupAsync("Unable to get player for this guild. Please try again later.");
        return;
      }

      var user = Context.User as IGuildUser;
      var voiceState = Context.User as IVoiceState;
      var textChannel = Context.Channel as ITextChannel;

      var trackCountBeforePlay = player.Queue.Count;

      var response = await musicPlaybackService.PlayTrackAsync(player.GuildId, player.VoiceChannelId, searchRequest);
      if (!response.IsSuccessful)
      {
        await DeleteOriginalResponseAsync();

        ModuleHelper.HandleCommandFailed(response);

        return;
      }



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

        if (player.Queue.Any() && player.CurrentTrack != null &&
            player.State is PlayerState.Playing or PlayerState.Paused)
        {
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

        await DeleteOriginalResponseAsync();
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PlayCommandAsync));
      throw;
    }
  }

  [SlashCommand(CommandMetadata.PauseCommandName, CommandMetadata.PauseCommandDescription, true, RunMode.Async)]
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

  [SlashCommand(CommandMetadata.ResumeCommandName, CommandMetadata.ResumeCommandDescription, true, RunMode.Async)]
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

  [SlashCommand(CommandMetadata.SeekCommandName, CommandMetadata.SeekCommandDescription, true, RunMode.Async)]
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

  [SlashCommand(CommandMetadata.SkipCommandName, CommandMetadata.SkipCommandDescription, true, RunMode.Async)]
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

  [SlashCommand(CommandMetadata.NowPlayingCommandName, CommandMetadata.NowPlayingCommandDescription, true, RunMode.Async)]
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

  [SlashCommand(CommandMetadata.ShuffleCommandName, CommandMetadata.ShuffleCommandDescription, true, RunMode.Async)]
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

  [SlashCommand(CommandMetadata.LyricsCommandName, CommandMetadata.LyricsCommandDescription, true, RunMode.Async)]
  [RequireOwner] // TODO: Since command doesn't work from Lavalink. Will be removed when it's fixed.
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

      var commandResponse = await musicService.GetLyricsFromTrackAsync(player);
      if (!commandResponse.IsSuccessful)
      {
        ModuleHelper.HandleCommandFailed(commandResponse);

        if (string.IsNullOrEmpty(commandResponse.Message))
        {
          await DeleteOriginalResponseAsync();
        }
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(LyricsCommandAsync));
      throw;
    }
  }

  [SlashCommand(CommandMetadata.QueueCommandName, CommandMetadata.QueueCommandDescription, true, RunMode.Async)]
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

  [SlashCommand(CommandMetadata.ClearCommandName, CommandMetadata.ClearCommandDescription, true, RunMode.Async)]
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

  [SlashCommand(CommandMetadata.AutoPlayCommandName, CommandMetadata.AutoPlayCommandDescription, true, RunMode.Async)]
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
