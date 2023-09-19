using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Lavalink4NET;
using Lavalink4NET.Clients.Events;
using Lavalink4NET.Events;
using Lavalink4NET.Events.Players;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Services;

public class LavaNodeService : ServiceBase<LavaNodeService>, ILavaNodeService
{
  private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
  private readonly IEmbedService _embedService;
  private readonly ILoggerAdapter<LavaNodeService> _logger;
  private readonly IMusicService _musicService;
  private readonly IVoiceService _voiceService;
  private readonly IAudioService _audioService;

  public LavaNodeService(IEmbedService embedService, IMusicService musicService, IVoiceService voiceService, IAudioService audioService, ILoggerAdapter<LavaNodeService> logger) : base(logger)
  {
    _embedService = embedService;
    _logger = logger;
    _musicService = musicService;
    _voiceService = voiceService;
    _audioService = audioService;
    _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
  }

  public new void Initialize()
  {
    if (_logger.IsLogLevelEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("{ServiceName} is initializing..", typeof(LavaNodeService).ToString());
    }

    _audioService.StatisticsUpdated += AudioServiceOnStatisticsUpdated;
    _audioService.TrackEnded += AudioServiceOnTrackEnded;
    _audioService.TrackException += AudioServiceOnTrackException;
    _audioService.TrackStarted += AudioServiceOnTrackStarted;
    _audioService.TrackStuck += AudioServiceOnTrackStuck;
    _audioService.WebSocketClosed += AudioServiceOnWebSocketClosed;

    _audioService.DiscordClient.VoiceServerUpdated += DiscordClientOnVoiceServerUpdated;
    _audioService.DiscordClient.VoiceStateUpdated += DiscordClientOnVoiceStateUpdated;
  }

  private Task DiscordClientOnVoiceStateUpdated(object sender, VoiceStateUpdatedEventArgs eventargs)
  {
    throw new NotImplementedException();
  }

  private Task DiscordClientOnVoiceServerUpdated(object sender, VoiceServerUpdatedEventArgs eventargs)
  {
    throw new NotImplementedException();
  }

  private Task AudioServiceOnWebSocketClosed(object sender, WebSocketClosedEventArgs eventargs)
  {
    _logger.LogCritical("Discord websocket has closed for the following reason: {Reason}", eventargs.Reason);

    return Task.CompletedTask;
  }

  private Task AudioServiceOnTrackStuck(object sender, TrackStuckEventArgs eventargs)
  {
    throw new System.NotImplementedException();
  }

  private Task AudioServiceOnTrackStarted(object sender, TrackStartedEventArgs eventargs)
  {
    throw new System.NotImplementedException();
  }

  private Task AudioServiceOnTrackException(object sender, TrackExceptionEventArgs eventargs)
  {
    throw new System.NotImplementedException();
  }

  private Task AudioServiceOnTrackEnded(object sender, TrackEndedEventArgs eventargs)
  {
    throw new System.NotImplementedException();
  }

  private Task AudioServiceOnStatisticsUpdated(object sender, StatisticsUpdatedEventArgs eventargs)
  {
    _logger.LogInformation("Lavalink has been online for: {Uptime}", eventargs.Statistics.Uptime);

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

  public async ValueTask DisposeAsync()
  {
    if (_audioService != null)
    {
      await _audioService.DisposeAsync();

      _audioService.StatisticsUpdated -= AudioServiceOnStatisticsUpdated;
      _audioService.TrackEnded -= AudioServiceOnTrackEnded;
      _audioService.TrackException -= AudioServiceOnTrackException;
      _audioService.TrackStarted -= AudioServiceOnTrackStarted;
      _audioService.TrackStuck -= AudioServiceOnTrackStuck;
      _audioService.WebSocketClosed -= AudioServiceOnWebSocketClosed;
    }

  }
}
