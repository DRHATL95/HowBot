using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Google.Apis.YouTube.v3;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET;
using Lavalink4NET.Integrations.Lavasearch;
using Lavalink4NET.Integrations.Lavasearch.Extensions;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Lyrics;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;

namespace Howbot.Core.Services;

public class MusicService : ServiceBase<MusicService>, IMusicService
{

  [NotNull] private readonly IEmbedService _embedService;
  
  [NotNull] private readonly ILoggerAdapter<MusicService> _logger;

  [NotNull] private readonly YouTubeService _youTubeService;

  [NotNull] private readonly IAudioService _audioService;

  [NotNull] private readonly ILyricsService _lyricsService;

  public MusicService([NotNull] IEmbedService embedService, [NotNull] YouTubeService youTubeService, [NotNull] IAudioService audioService, [NotNull] ILoggerAdapter<MusicService> logger, [NotNull] ILyricsService lyricsService) : base(logger)
  {
    _embedService = embedService;
    _youTubeService = youTubeService;
    _logger = logger;
    _lyricsService = lyricsService;
    _audioService = audioService;
  }

  public async Task<IEnumerable<string>> GetYoutubeRecommendedVideoId(string videoId, int count = 1)
  {
    var searchListRequest = _youTubeService.Search.List("snippet");

    searchListRequest.Type = "video";
    // For some reason if I want 1 result, I have to set the max results to 2?
    // TODO: Investigate
    searchListRequest.MaxResults = count == 1 ? 2 : count;
    searchListRequest.RelatedToVideoId = videoId;

    var response = await searchListRequest.ExecuteAsync();

    if (count <= 1)
    {
      return new[] { response.Items[0].Id.VideoId };
    }

    return response.Items.Select(item => item.Id.VideoId).ToList();
  }


  #region Music Module Commands

  public async Task<CommandResponse> PlayTrackBySearchTypeAsync<T>(T player, SearchProviderTypes searchProviderType, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel) where T : ILavalinkPlayer
  {
    try
    {
      var type = ConvertSearchProviderTypeToTrackSearchMode(searchProviderType);

      type = TrackSearchMode.YouTube;

      var searchResponse = await _audioService.Tracks.SearchAsync(query: searchRequest,
        loadOptions: new TrackLoadOptions(SearchMode: type),
        categories: ImmutableArray.Create(SearchCategory.Track)).ConfigureAwait(false);

      if (searchResponse is null) return CommandResponse.CommandNotSuccessful("Unable to find any tracks");

      var extendedLavalinkTrack = new ExtendedLavalinkTrack(searchResponse.Tracks[0]);

      await player.PlayAsync(extendedLavalinkTrack.Track).ConfigureAwait(false);

      return CommandResponse.CommandSuccessful(extendedLavalinkTrack.Track);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(PlayTrackBySearchTypeAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> PauseTrackAsync<T>(T player) where T : ILavalinkPlayer
  {
    try
    {
      _logger.LogDebug("Pausing current track [{GuildId}]", player.GuildId);

      await player.PauseAsync().ConfigureAwait(false);

      return CommandResponse.CommandSuccessful();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(PlayTrackBySearchTypeAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> ResumeTrackAsync<T>(T player) where T : ILavalinkPlayer
  {
    try
    {
      if (player.State is not PlayerState.Paused)
      {
        return CommandResponse.CommandNotSuccessful("Player is not paused.");
      }

      await player.ResumeAsync().ConfigureAwait(false);

      return CommandResponse.CommandSuccessful();

    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(ResumeTrackAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> SkipTrackAsync<T>(T player, int numberOfTracks) where T : ILavalinkPlayer
  {
    try
    {
      if (player is IQueuedLavalinkPlayer { State: PlayerState.Playing } queuedPlayer)
      {
        await queuedPlayer.SkipAsync(numberOfTracks);

        return CommandResponse.CommandSuccessful($"Skipped {numberOfTracks} tracks in queue.");
      }

      return CommandResponse.CommandNotSuccessful("Unable to skip to position in queue.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown in MusicService.SkipTrackAsync");
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> SeekTrackAsync<T>(T player, TimeSpan seekPosition) where T : ILavalinkPlayer
  {
    try
    {
      _logger.LogDebug($"Seeking to {seekPosition:g}.");

      await player.SeekAsync(seekPosition).ConfigureAwait(false);

      return CommandResponse.CommandSuccessful($"Seeking to {seekPosition:g}.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> ChangeVolumeAsync<T>(T player, int? newVolume) where T : ILavalinkPlayer
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
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task<CommandResponse> NowPlayingAsync<T>(T player, IGuildUser user, ITextChannel textChannel) where T : ILavalinkPlayer
  {
    try
    {
      if (player.CurrentTrack is null)
      {
        return CommandResponse.CommandNotSuccessful("No track is currently playing.");
      }

      var embed = await _embedService.GenerateMusicNowPlayingEmbedAsync(player.CurrentTrack, user, textChannel);

      return CommandResponse.CommandSuccessful(embed);

    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public Task<CommandResponse> ApplyAudioFilterAsync<T>(T player, IPlayerFilters filter) where T : ILavalinkPlayer
  {
    try
    {
      // Create filter options
      /*var options = new EchoFilterOptions
      {
        Delay = 1.0F,
      };

      player.Filters.Echo(options);*/

      return Task.FromResult(CommandResponse.CommandSuccessful());
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return Task.FromResult(CommandResponse.CommandNotSuccessful(exception));
    }
  }

  public async Task<CommandResponse> GetLyricsFromTrackAsync<T>(T player) where T: ILavalinkPlayer
  {
    try
    {
      var track = player.CurrentTrack;

      if (track is null)
      {
        return CommandResponse.CommandNotSuccessful("Nothing is playing.");
      }

      var lyrics = await _lyricsService.GetLyricsAsync(track.Author, track.Title);

      return string.IsNullOrEmpty(lyrics) ? CommandResponse.CommandNotSuccessful("No lyrics found for current song playing.") : CommandResponse.CommandSuccessful(lyrics);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
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

      return CommandResponse.CommandSuccessful($"Shuffle is now {(queuedLavalinkPlayer.Shuffle ? "enabled" : "disabled")}.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }


  /*public CommandResponse ToggleTwoFourSeven(IGuild guild)
  {
    try
    {
      if (!_lavaNode.TryGetPlayer(guild, out var lavaPlayer))
      {
        _logger.LogDebug(ClientNotConnectedToVoiceChannel);
        return CommandResponse.CommandNotSuccessful(BotNotConnectedToVoiceResponseMessage);
      }

      _logger.LogDebug(lavaPlayer.Is247ModeEnabled ? TwoFourSevenOff : TwoFourSevenOn);

      lavaPlayer.Toggle247Mode();

      var response = lavaPlayer.Is247ModeEnabled ? BotTwoFourSevenOn : BotTwoFourSevenOff;
      return CommandResponse.CommandSuccessful(response);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }*/

  #endregion Music Module Commands

  private static TrackSearchMode ConvertSearchProviderTypeToTrackSearchMode(SearchProviderTypes searchProviderType)
  {
    return searchProviderType switch
    {
      SearchProviderTypes.Apple => TrackSearchMode.AppleMusic,
      SearchProviderTypes.Deezer => TrackSearchMode.Deezer,
      SearchProviderTypes.SoundCloud => TrackSearchMode.SoundCloud,
      SearchProviderTypes.Spotify => TrackSearchMode.Spotify,
      SearchProviderTypes.YouTube => TrackSearchMode.YouTube,
      SearchProviderTypes.YouTubeMusic => TrackSearchMode.YouTubeMusic,
      SearchProviderTypes.YandexMusic => TrackSearchMode.YandexMusic,
      _ => TrackSearchMode.YouTube
    };
  }
}
