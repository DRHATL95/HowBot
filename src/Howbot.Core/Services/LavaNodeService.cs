using System;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using JetBrains.Annotations;
using Lavalink4NET;
using Lavalink4NET.Clients.Events;
using Lavalink4NET.Events;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Protocol.Payloads.Events;

namespace Howbot.Core.Services;

public class LavaNodeService : ServiceBase<LavaNodeService>, ILavaNodeService, IAsyncDisposable
{
  private readonly IAudioService _audioService;

  public LavaNodeService(IAudioService audioService, ILoggerAdapter<LavaNodeService> logger) : base(logger)
  {
    _audioService = audioService;
  }

  public async ValueTask DisposeAsync()
  {
    await _audioService.DisposeAsync();

    _audioService.StatisticsUpdated -= AudioServiceOnStatisticsUpdated;
    _audioService.TrackEnded -= AudioServiceOnTrackEnded;
    _audioService.TrackException -= AudioServiceOnTrackException;
    _audioService.TrackStarted -= AudioServiceOnTrackStarted;
    _audioService.TrackStuck -= AudioServiceOnTrackStuck;
    _audioService.WebSocketClosed -= AudioServiceOnWebSocketClosed;

    GC.SuppressFinalize(this);
  }

  public override void Initialize()
  {
    Logger.LogDebug("{ServiceName} is initializing..", nameof(LavaNodeService));

    _audioService.StatisticsUpdated += AudioServiceOnStatisticsUpdated;
    _audioService.TrackEnded += AudioServiceOnTrackEnded;
    _audioService.TrackException += AudioServiceOnTrackException;
    _audioService.TrackStarted += AudioServiceOnTrackStarted;
    _audioService.TrackStuck += AudioServiceOnTrackStuck;
    _audioService.WebSocketClosed += AudioServiceOnWebSocketClosed;

    // Discord Client Wrapper (Lavalink4Net) Events
    _audioService.DiscordClient.VoiceServerUpdated += DiscordClientOnVoiceServerUpdated;
    _audioService.DiscordClient.VoiceStateUpdated += DiscordClientOnVoiceStateUpdated;
  }

  private Task DiscordClientOnVoiceStateUpdated(object sender, VoiceStateUpdatedEventArgs eventargs)
  {
    Logger.LogDebug("Voice state updated.");

    return Task.CompletedTask;
  }

  private Task DiscordClientOnVoiceServerUpdated(object sender, VoiceServerUpdatedEventArgs eventargs)
  {
    Logger.LogDebug("Voice server updated.");

    return Task.CompletedTask;
  }

  private Task AudioServiceOnWebSocketClosed(object sender, WebSocketClosedEventArgs eventargs)
  {
    Logger.LogCritical("Discord websocket has closed for the following reason: {Reason}", eventargs.Reason);

    return Task.CompletedTask;
  }

  private Task AudioServiceOnTrackStuck(object sender, TrackStuckEventArgs eventargs)
  {
    Logger.LogInformation("Track {TrackName} is stuck while trying to play.", eventargs.Track.Identifier);

    return Task.CompletedTask;
  }

  private Task AudioServiceOnTrackStarted(object sender, TrackStartedEventArgs eventargs)
  {
    Logger.LogDebug("Starting track [{TrackTitle}]", eventargs.Track.Title);

    return Task.CompletedTask;
  }

  private Task AudioServiceOnTrackException(object sender, TrackExceptionEventArgs eventargs)
  {
    var exception = new Exception(eventargs.Exception.Message);

    Logger.LogError(exception, "[{Severity}] - LavaNode has thrown an exception",
      eventargs.Exception.Severity);

    return Task.CompletedTask;
  }

  private Task AudioServiceOnTrackEnded(object sender, TrackEndedEventArgs eventargs)
  {
    if (eventargs.Reason is not TrackEndReason.Finished)
    {
      Logger.LogInformation("Track [{TrackName}] has ended with reason [{Reason}]", eventargs.Track.Title,
        eventargs.Reason);
      return Task.CompletedTask;
    }

    Logger.LogDebug("Current song [{SongName}] has ended.", eventargs.Track.Title);

    return Task.CompletedTask;
  }

  private Task AudioServiceOnStatisticsUpdated(object sender, StatisticsUpdatedEventArgs eventargs)
  {
    Logger.LogDebug(eventargs.Statistics.ToString());

    return Task.CompletedTask;
  }

  /*private async Task<LavalinkTrack> GetUniqueRadioTrack(ILavalinkPlayer player, int attempt = 0)
  {
    ArgumentNullException.ThrowIfNull(player);

    List<string> uniqueVideoIds;

    // Recursive base case
    if (attempt >= Constants.MaximumUniqueSearchAttempts)
    {
      _logger.LogDebug("Unable to find a song after {SearchLimit} attempts.", Constants.MaximumUniqueSearchAttempts);
      return null;
    }

    if (player.LastPlayed == null && !player.RecentlyPlayed.Any())
    {
      _logger.LogError("Unable to find another song, last song is null.");
      return null;
    }

    if (attempt > 0)
    {
      // Recursive pass
      uniqueVideoIds = (await _musicService.GetYoutubeRecommendedVideoId(player.LastPlayed.Id, Constants.RelatedSearchResultsLimit + attempt)).ToList();
    }
    else
    {
      // First pass
      uniqueVideoIds = (await _musicService.GetYoutubeRecommendedVideoId(player.LastPlayed.Id, Constants.RelatedSearchResultsLimit)).ToList();
    }

    var videoId = uniqueVideoIds.FirstOrDefault(videoId => player.RecentlyPlayed.Any(track => track.Id != videoId));
    if (string.IsNullOrEmpty(videoId))
    {
      // Recursive call
      return await GetUniqueRadioTrack(player, ++attempt);
    }

    var videoUrl = Constants.YouTubeBaseShortUrl + videoId;
    var searchResult = await _lavaNode.SearchAsync(SearchType.Direct, videoUrl);
    if (!searchResult.Tracks.Any())
    {
      _logger.LogError("Unable to find a track using Victoria search.");
      return null;
    }

    var nextTrack = searchResult.Tracks.First();
    if (player.RecentlyPlayed.Any(x => x.Id == nextTrack.Id))
    {
      // Recursive call
      return await GetUniqueRadioTrack(player, ++attempt);
    }

    if (MusicHelper.AreTracksSimilar(player.LastPlayed, nextTrack))
    {
      // Recursive call
      return await GetUniqueRadioTrack(player, ++attempt);
    }

    _logger.LogDebug("{TrackName} was returned.", nextTrack.Title);

    return nextTrack;
  }*/
}
