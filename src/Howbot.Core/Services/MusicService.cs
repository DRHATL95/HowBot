using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Players;
using JetBrains.Annotations;
using Lavalink4NET;
using Lavalink4NET.Clients;
using Lavalink4NET.Integrations.Lavasearch;
using Lavalink4NET.Integrations.Lavasearch.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Howbot.Core.Services;

public class MusicService : ServiceBase<MusicService>, IMusicService
{
  [NotNull] private readonly IAudioService _audioService;

  [NotNull] private readonly IEmbedService _embedService;

  public MusicService([NotNull] IEmbedService embedService, [NotNull] IAudioService audioService)
  {
    _embedService = embedService;
    _audioService = audioService;
  }

  [ItemCanBeNull]
  public async ValueTask<HowbotPlayer> GetPlayerByContextAsync(SocketInteractionContext context,
    bool allowConnect = false, bool requireChannel = true, ImmutableArray<IPlayerPrecondition> preconditions = default,
    bool isDeferred = false,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    // Can't execute function without socket context
    ArgumentNullException.ThrowIfNull(context, nameof(context));

    if (context.User is not SocketGuildUser guildUser)
    {
      await context.Interaction.FollowupAsync("Unable to create player, command requested by non-guild member.")
        .ConfigureAwait(false);

      return null;
    }

    ulong voiceChannelId = guildUser.VoiceChannel?.Id ?? 0;
    IGuild guild = context.Guild;
    ulong guildId = guild.Id;

    var retrieveOptions = new PlayerRetrieveOptions(
      ChannelBehavior: allowConnect ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None,
      VoiceStateBehavior: requireChannel ? MemberVoiceStateBehavior.RequireSame : MemberVoiceStateBehavior.Ignore,
      Preconditions: preconditions);

    HowbotPlayerOptions playerOptions = new HowbotPlayerOptions()
    {
      DisconnectOnDestroy = true,
      DisconnectOnStop = true,
      SelfDeaf = true,
      ClearQueueOnStop = true,
      ClearHistoryOnStop = true
    };

    var result = await _audioService.Players.RetrieveAsync<HowbotPlayer, HowbotPlayerOptions>(guildId, voiceChannelId,
        CreatePlayerAsync,
        retrieveOptions: retrieveOptions, options: new OptionsWrapper<HowbotPlayerOptions>(playerOptions),
        cancellationToken: cancellationToken)
      .ConfigureAwait(false);

    if (!result.IsSuccess)
    {
      var errorMessage = result.Status switch
      {
        // The user is not connected to a voice channel.
        PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",

        // The bot is not in a voice channel
        PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected to a voice channel.",

        // The bot is not in the same voice channel as the user.
        PlayerRetrieveStatus.VoiceChannelMismatch => "You are not in the same voice channel as the bot.",

        // The bot failed it's precondition check.
        PlayerRetrieveStatus.PreconditionFailed => "The bot failed it's precondition check.",

        _ => "An unknown error occurred while creating the player."
      };

      await context.Interaction.FollowupAsync(errorMessage).ConfigureAwait(false);

      return null;
    }

    return result.Player;
  }

  public ValueTask<IEnumerable<string>> GetYoutubeRecommendedVideoId(string videoId, int count = 1)
  {
    throw new NotImplementedException();
  }

  private static ValueTask<HowbotPlayer> CreatePlayerAsync(
    IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    ArgumentNullException.ThrowIfNull(properties);

    Log.Logger.Information("Creating new player..");

    return ValueTask.FromResult(new HowbotPlayer(properties));
  }

  #region Music Module Commands

  public async ValueTask<CommandResponse> PlayTrackBySearchTypeAsync(HowbotPlayer player,
    SearchProviderTypes searchProviderType, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel)
  {
    try
    {
      // Convert from enum to Lavalink struct for searching providers (default is YouTube)
      var type = LavalinkHelper.ConvertSearchProviderTypeToTrackSearchMode(searchProviderType);

      var trackOptions = new TrackLoadOptions(type);

      // LavaSearch categories to be returned (Tracks, Albums, Artists, etc.)
      var categories = ImmutableArray.Create(SearchCategory.Track);

      var searchResult = await _audioService.Tracks
        .SearchAsync(searchRequest, loadOptions: trackOptions, categories: categories)
        .ConfigureAwait(false);

      LavalinkTrack track;
      if (searchResult is null || searchResult.Tracks.IsDefaultOrEmpty)
      {
        // Attempts to use native lavalink native search when lavasearch plugin isn't working or doesn't return results for categories specified
        track = await _audioService.Tracks
          .LoadTrackAsync(searchRequest, trackOptions)
          .ConfigureAwait(false);
      }
      else
      {
        track = searchResult.Tracks[0];
      }

      if (track != null)
      {
        await player.PlayAsync(track).ConfigureAwait(false);

        return CommandResponse.CommandSuccessful(track);
      }
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(PlayTrackBySearchTypeAsync));
    }

    return CommandResponse.CommandNotSuccessful(Messages.Responses.CommandPlayNotSuccessfulResponse);
  }

  public async ValueTask<CommandResponse> PauseTrackAsync(HowbotPlayer player)
  {
    try
    {
      Logger.LogDebug("[{GuildId}] - Pausing current track.", player.GuildId);

      await player.PauseAsync().ConfigureAwait(false);

      return CommandResponse.CommandSuccessful(Messages.Responses.CommandPausedSuccessfulResponse);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(PlayTrackBySearchTypeAsync));
      return CommandResponse.CommandNotSuccessful(Messages.Responses.CommandPausedNotSuccessfulResponse);
    }
  }

  public async ValueTask<CommandResponse> ResumeTrackAsync(HowbotPlayer player)
  {
    try
    {
      await player.ResumeAsync().ConfigureAwait(false);

      return CommandResponse.CommandSuccessful();
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(ResumeTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async ValueTask<CommandResponse> SkipTrackAsync(HowbotPlayer player, int? numberOfTracks)
  {
    try
    {
      if (player is IQueuedLavalinkPlayer { State: PlayerState.Playing } queuedPlayer)
      {
        await queuedPlayer.SkipAsync(numberOfTracks ?? 1);

        return CommandResponse.CommandSuccessful($"Skipped {numberOfTracks} tracks in queue.");
      }

      return CommandResponse.CommandNotSuccessful("Unable to skip to position in queue.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, "Exception thrown in MusicService.SkipTrackAsync");
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async ValueTask<CommandResponse> SeekTrackAsync(HowbotPlayer player, TimeSpan seekPosition)
  {
    try
    {
      Logger.LogDebug($"Seeking to {seekPosition:g}.");

      await player.SeekAsync(seekPosition).ConfigureAwait(false);

      return CommandResponse.CommandSuccessful($"Seeking to {seekPosition:g}.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async ValueTask<CommandResponse> ChangeVolumeAsync(HowbotPlayer player, int? newVolume)
  {
    try
    {
      // Since second param is optional, if 0 do nothing but return command success
      if (!newVolume.HasValue)
      {
        return CommandResponse.CommandSuccessful();
      }

      if (player is not QueuedLavalinkPlayer queuedLavalinkPlayer)
      {
        return CommandResponse.CommandNotSuccessful("Unable to change volume.");
      }

      await queuedLavalinkPlayer.SetVolumeAsync(newVolume.Value / 100f).ConfigureAwait(false);
      return CommandResponse.CommandSuccessful($"Volume set to {newVolume}%");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async ValueTask<CommandResponse> NowPlayingAsync(HowbotPlayer player, IGuildUser user,
    ITextChannel textChannel)
  {
    try
    {
      if (player.CurrentTrack is null)
      {
        return CommandResponse.CommandNotSuccessful("No track is currently playing.");
      }

      var embed = await _embedService.GenerateMusicNowPlayingEmbedAsync(player.CurrentTrack, user, textChannel,
        player.Position?.Position);

      return CommandResponse.CommandSuccessful(embed);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public ValueTask<CommandResponse> ApplyAudioFilterAsync(HowbotPlayer player, IPlayerFilters filter)
  {
    try
    {
      // Create filter options
      /*var options = new EchoFilterOptions
      {
        Delay = 1.0F,
      };

      player.Filters.Echo(options);*/

      return ValueTask.FromResult(CommandResponse.CommandSuccessful());
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return ValueTask.FromResult(CommandResponse.CommandNotSuccessful(exception));
    }
  }

  public ValueTask<CommandResponse> GetLyricsFromTrackAsync(HowbotPlayer player)
  {
    throw new NotImplementedException();
  }

  public CommandResponse ToggleShuffle(HowbotPlayer player)
  {
    throw new NotImplementedException();
  }

  public CommandResponse ToggleShuffle<T>(T player) where T : ILavalinkPlayer
  {
    try
    {
      if (player is not IQueuedLavalinkPlayer queuedLavalinkPlayer)
      {
        return CommandResponse.CommandNotSuccessful("Unable to shuffle queue.");
      }

      queuedLavalinkPlayer.Shuffle = !queuedLavalinkPlayer.Shuffle;

      return CommandResponse.CommandSuccessful(
        $"Shuffle is now {(queuedLavalinkPlayer.Shuffle ? "enabled" : "disabled")}.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(SeekTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public CommandResponse ToggleTwoFourSeven(HowbotPlayer player)
  {
    throw new NotImplementedException();
  }

  #endregion Music Module Commands
}
