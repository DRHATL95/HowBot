using System.Text.RegularExpressions;
using Howbot.Application.Constants;
using Howbot.Application.Interfaces.Infrastructure;
using Howbot.Application.Interfaces.Lavalink;
using Howbot.Application.Models.Lavalink;
using Howbot.Domain.Entities.Concrete;
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
  [GeneratedRegex(RegexPatterns.UrlPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
  private static partial Regex UrlPattern();


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

      var player = await playerFactory.GetOrCreatePlayerAsync(guildId, voiceChannelId, token: ct);
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

      logger.LogDebug("Playing track: {TrackTitle} for guild {GuildId} in voice channel {VoiceChannelId}", trackQueueItem!.Track?.Title ?? string.Empty, guildId, voiceChannelId);

      await player.PlayAsync(trackQueueItem!, false, cancellationToken: ct);

      return MusicCommandResult.Success();
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PlayTrackAsync));
      return MusicCommandResult.Failure("An error occurred while trying to play the track. Please try again later.");
    }
  }

  public async ValueTask<MusicCommandResult> PauseTrackAsync(ulong guildId, ulong voiceChannelId, CancellationToken ct = default)
  {
    try
    {
      var player = await playerFactory.GetOrCreatePlayerAsync(guildId, voiceChannelId, token: ct);
      if (player == null)
      {
        return MusicCommandResult.Failure("Failed to retrieve the player for the specified guild.");
      }

      if (player.State != PlayerState.Playing)
      {
        return MusicCommandResult.Failure("The player is not currently playing any track.");
      }

      logger.LogDebug("Pausing track for guild {GuildId} in voice channel {VoiceChannelId}", guildId, voiceChannelId);

      await player.PauseAsync(cancellationToken: ct);

      return MusicCommandResult.Success("Track has been paused successfully.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(PauseTrackAsync));
      return MusicCommandResult.Failure("An error occurred while trying to pause the track. Please try again later.");
    }
  }

  public async ValueTask<MusicCommandResult> ResumeTrackAsync(ulong guildId, ulong voiceChannelId, CancellationToken ct = default)
  {
    try
    {
      var player = await playerFactory.GetOrCreatePlayerAsync(guildId, voiceChannelId, token: ct);
      if (player == null)
      {
        return MusicCommandResult.Failure("Failed to retrieve the player for the specified guild.");
      }

      logger.LogDebug("Resuming track for guild {GuildId} in voice channel {VoiceChannelId}", guildId, voiceChannelId);

      await player.ResumeAsync(cancellationToken: ct);

      return player.State == PlayerState.Playing
        ? MusicCommandResult.Success("Track has been resumed successfully.")
        : MusicCommandResult.Failure("Failed to resume the track. The player is not in a valid state.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(ResumeTrackAsync));
      return MusicCommandResult.Failure("An error occurred while trying to resume the track. Please try again later.");
    }
  }

  public async ValueTask<MusicCommandResult> SkipTrackAsync(ulong guildId, ulong voiceChannelId, int? numberOfTracks = null, CancellationToken ct = default)
  {
    try
    {
      var player = await playerFactory.GetOrCreatePlayerAsync(guildId, voiceChannelId, token: ct);
      if (player == null)
      {
        return MusicCommandResult.Failure("Failed to retrieve the player for the specified guild.");
      }

      logger.LogDebug("Skipping track. Number of tracks to skip: {NumberOfTracks}", numberOfTracks ?? 1);

      await player.SkipAsync(numberOfTracks ?? 1, cancellationToken: ct);

      return MusicCommandResult.Success("Track has been skipped successfully.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(SkipTrackAsync));
      return MusicCommandResult.Failure("An error occurred while trying to skip the track. Please try again later.");
    }
  }

  public async ValueTask<MusicCommandResult> SeekTrackAsync(ulong guildId, ulong voiceChannelId, TimeSpan seekPosition, CancellationToken ct = default)
  {
    try
    {
      var player = await playerFactory.GetOrCreatePlayerAsync(guildId, voiceChannelId, token: ct);
      if (player == null)
      {
        return MusicCommandResult.Failure("Failed to retrieve the player for the specified guild.");
      }

      logger.LogDebug("Seeking track to position: {SeekPosition}", seekPosition);

      await player.SeekAsync(seekPosition, cancellationToken: ct);

      return MusicCommandResult.Success($"Track has been seeked to {seekPosition:mm\\:ss} successfully.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(SeekTrackAsync));
      return MusicCommandResult.Failure("An error occurred while trying to seek the track. Please try again later.");
    }
  }

  public async ValueTask<MusicCommandResult> ChangeVolumeAsync(ulong guildId, ulong voiceChannelId, int volume, CancellationToken ct = default)
  {
    try
    {
      if (volume < 0 || volume > 100)
      {
        return MusicCommandResult.Failure("Volume must be between 0 and 100.");
      }

      var player = await playerFactory.GetOrCreatePlayerAsync(guildId, voiceChannelId, token: ct);
      if (player == null)
      {
        return MusicCommandResult.Failure("Failed to retrieve the player for the specified guild.");
      }

      logger.LogDebug("Changing volume to: {Volume}", volume);

      await player.SetVolumeAsync(volume, cancellationToken: ct);

      using var scope = serviceProvider.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<IDatabaseService>();

      if (db.DoesGuildExist(guildId))
      {
        await db.UpdatePlayerVolumeLevelAsync(guildId, volume, ct);
      }
      else
      {
        db.AddNewGuild(new Guild { GuildId = player.GuildId, Volume = volume });
      }

      return MusicCommandResult.Success($"Volume has been set to {volume}% successfully.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(ChangeVolumeAsync));
      return MusicCommandResult.Failure("An error occurred while trying to change the volume. Please try again later.");
    }
  }

  public ValueTask<MusicCommandResult> ApplyAudioFilterAsync(ulong guildId, ulong voiceChannelId, IPlayerFilters filters, CancellationToken ct = default)
  {
    try
    {
      var player = playerFactory.GetOrCreatePlayerAsync(guildId, voiceChannelId, token: ct).AsTask().Result;
      if (player == null)
      {
        return new ValueTask<MusicCommandResult>(MusicCommandResult.Failure("Failed to retrieve the player for the specified guild."));
      }

      logger.LogDebug("Applying audio filters for guild {GuildId} in voice channel {VoiceChannelId}", guildId, voiceChannelId);

      return new ValueTask<MusicCommandResult>(MusicCommandResult.Success());

      // return new ValueTask<MusicCommandResult>(MusicCommandResult.Success("Audio filters have been applied successfully."));
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(ApplyAudioFilterAsync));
      return new ValueTask<MusicCommandResult>(MusicCommandResult.Failure("An error occurred while trying to apply audio filters. Please try again later."));
    }
  }

  public ValueTask<MusicCommandResult> GetLyricsAsync(ulong guildId, ulong voiceChannelId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

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
