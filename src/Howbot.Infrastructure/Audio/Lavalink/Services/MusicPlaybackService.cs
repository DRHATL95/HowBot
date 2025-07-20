using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Howbot.Application.Constants;
using Howbot.Application.Enums;
using Howbot.Application.Interfaces.Infrastructure;
using Howbot.Application.Interfaces.Lavalink;
using Howbot.Application.Models.Lavalink;
using Howbot.Infrastructure.Audio.Helpers;
using Howbot.SharedKernel;
using Howbot.SharedKernel.Constants;
using Lavalink4NET;
using Lavalink4NET.Integrations.Lavasearch;
using Lavalink4NET.Integrations.Lavasearch.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.DependencyInjection;
using CacheMode = Lavalink4NET.Rest.Entities.CacheMode;

namespace Howbot.Infrastructure.Audio.Lavalink.Services;

public partial class MusicPlaybackService(IAudioService audioService, IPlayerFactoryService playerFactory, IServiceProvider serviceProvider, ILoggerAdapter<MusicPlaybackService> logger) : IMusicPlaybackService
{
  public async ValueTask<MusicCommandResult> PlayTrackAsync(ulong guildId, ulong voiceChannelId, string query, CancellationToken ct = default)
  {
    try
    {
      using var scope = serviceProvider.CreateScope();
      var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

      var searchProviderType = BotDefaults.DefaultSearchProvider;
      if (databaseService.DoesGuildExist(guildId))
      {
        searchProviderType = databaseService.GetGuildSearchProviderType(guildId);
      }

      // If the search request is a URL, don't use the LavaSearch plugin
      var trackSearchMode = UrlPattern().IsMatch(query)
        ? TrackSearchMode.None // URL search sets to None
        : LavalinkHelper.ConvertSearchProviderTypeToTrackSearchMode(searchProviderType);

      var tracks = await GetLavalinkTrackBySearchQueryAsync(query, trackSearchMode, ct: ct);
      if (tracks.Count == 0)
      {
        return MusicCommandResult.Failure("No tracks have been found for the given search query.");
      }

      var player = await playerFactory.GetOrCreatePlayerAsync(guildId, voiceChannelId, ct);
      if (player == null)
      {
        return MusicCommandResult.Failure("Failed to create or retrieve the player for the specified guild.");
      }
      
      var trackQueueItems = tracks.Select(track => new TrackQueueItem(new TrackReference(track)));
      var amountOfTracksAdded = await player.Queue.AddRangeAsync((IReadOnlyList<ITrackQueueItem>)trackQueueItems, ct);

      // If the player is already playing
      if (player.State is PlayerState.Playing or PlayerState.Paused)
      {
        return amountOfTracksAdded > 1
          ? MusicCommandResult.Success($"Added {amountOfTracksAdded} tracks to the queue.")
          : MusicCommandResult.Success($"Added track: {tracks[0].Title} to the queue.");
      }
      
      var trackQueueItem = await player.Queue.TryDequeueAsync(player.Shuffle ? TrackDequeueMode.Shuffle : TrackDequeueMode.Normal, ct);
      if (trackQueueItem == null)
      {
        return MusicCommandResult.Failure("Failed to dequeue a track from the queue.");
      }

      await player.PlayAsync(trackQueueItem, false, cancellationToken: ct);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PlayTrackAsync));
      return MusicCommandResult.Failure("An error occurred while trying to play the track. Please try again later.");
    }
  }

  public async ValueTask<MusicCommandResult> PauseTrackAsync(ulong guildId, CancellationToken ct = default)
  {
    try
    {
      var player = await playerFactory.GetOrCreatePlayerAsync(guildId, ct: ct);
      if (player == null)
      {
        return MusicCommandResult.Failure("Failed to retrieve the player for the specified guild.");
      }

      if (player.State != PlayerState.Playing)
      {
        return MusicCommandResult.Failure("The player is not currently playing any track.");
      }

      await player.PauseAsync(cancellationToken: ct);
      return MusicCommandResult.Success("Track has been paused successfully.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PauseTrackAsync));
      return MusicCommandResult.Failure("An error occurred while trying to pause the track. Please try again later.");
    }
  }

  public ValueTask<MusicCommandResult> ResumeTrackAsync(ulong guildId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> SkipTrackAsync(ulong guildId, int? numberOfTracks = null, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> SeekTrackAsync(ulong guildId, TimeSpan seekPosition, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> ChangeVolumeAsync(ulong guildId, int volume, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> ApplyAudioFilterAsync(ulong guildId, IPlayerFilters filters, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> GetLyricsAsync(ulong guildId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> ToggleShuffleAsync(ulong guildId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<MusicCommandResult> GetQueueAsync(ulong guildId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<string> GetSpotifyRecommendationAsync(LavalinkTrack track, string market = "US", int limit = 10,
    CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  [GeneratedRegex(RegexPatterns.UrlPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
  private static partial Regex UrlPattern();

  private async ValueTask<SearchResult?> SearchForTrackBySearchQueryAsync(string searchQuery, TrackLoadOptions trackLoadOptions = default, CancellationToken ct = default)
  {
    if (string.IsNullOrEmpty(searchQuery))
    {
      logger.LogError("Search query is empty");
      return null;
    }
    
    logger.LogDebug($"Searching for track with search query: {searchQuery}");
    
    var searchResult = await audioService.Tracks
      .SearchAsync(searchQuery, [SearchCategory.Track], trackLoadOptions, cancellationToken: ct);
    
    return searchResult;
  }

  private async ValueTask<IList<LavalinkTrack>> GetLavalinkTrackBySearchQueryAsync(string searchQuery,
    TrackSearchMode trackSearchMode = default, CancellationToken ct = default)
  {
    if (string.IsNullOrWhiteSpace(searchQuery))
    {
      logger.LogError("Search query is empty or null");
      return Array.Empty<LavalinkTrack>();
    }
    
    TrackLoadOptions trackLoadOptions = new TrackLoadOptions
    {
      SearchMode = trackSearchMode, SearchBehavior = StrictSearchBehavior.Resolve, CacheMode = CacheMode.Dynamic
    };
    
    // Attempt to use Lavasearch initially
    var searchResult = await SearchForTrackBySearchQueryAsync(searchQuery, trackLoadOptions, ct);
    if (searchResult != null)
    {
      return searchResult.Tracks.ToList();
    }
    
    bool isMultipleTracks = ShouldSearchQueryReturnMultipleTracks(searchQuery);
    
    // If Lavasearch didn't return results, fallback to Lavalink's default load track
    var tracks = new List<LavalinkTrack>();

    if (isMultipleTracks)
    {
      tracks = (await audioService.Tracks
        .LoadTracksAsync(searchQuery, trackLoadOptions, cancellationToken: ct)).Tracks.ToList();
    }
    else
    {
      var track = await audioService.Tracks
        .LoadTrackAsync(searchQuery, trackLoadOptions, cancellationToken: ct);

      if (track == null)
      {
        return tracks;
      }

      logger.LogDebug("Track found: {TrackTitle}", track.Title);
      tracks.Add(track);
    }
    
    return tracks;
  }

  private static bool ShouldSearchQueryReturnMultipleTracks(string searchQuery)
  {
    return searchQuery.Contains("playlist", StringComparison.CurrentCultureIgnoreCase) ||
           searchQuery.Contains("album", StringComparison.CurrentCultureIgnoreCase);
  }
}
