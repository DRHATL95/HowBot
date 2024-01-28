using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using Howbot.Core.Models.Players;
using Lavalink4NET;
using Lavalink4NET.Clients.Events;
using Lavalink4NET.Events;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Protocol.Payloads.Events;

namespace Howbot.Core.Services;

public class LavaNodeService(IAudioService audioService, ILoggerAdapter<LavaNodeService> logger, IEmbedService embedService)
  : ServiceBase<LavaNodeService>(logger), ILavaNodeService, IAsyncDisposable
{
  public override void Initialize()
  {
    base.Initialize();

    audioService.StatisticsUpdated += AudioServiceOnStatisticsUpdated;
    audioService.TrackEnded += AudioServiceOnTrackEnded;
    audioService.TrackException += AudioServiceOnTrackException;
    audioService.TrackStarted += AudioServiceOnTrackStarted;
    audioService.TrackStuck += AudioServiceOnTrackStuck;
    audioService.WebSocketClosed += AudioServiceOnWebSocketClosed;

    // Discord Client Wrapper (Lavalink4Net) Events
    audioService.DiscordClient.VoiceServerUpdated += DiscordClientOnVoiceServerUpdated;
    audioService.DiscordClient.VoiceStateUpdated += DiscordClientOnVoiceStateUpdated;
  }

  #region Events

  private Task DiscordClientOnVoiceStateUpdated(object sender, VoiceStateUpdatedEventArgs eventArgs)
  {
    Logger.LogDebug("Voice state updated.");

    return Task.CompletedTask;
  }

  private Task DiscordClientOnVoiceServerUpdated(object sender, VoiceServerUpdatedEventArgs eventArgs)
  {
    Logger.LogDebug("Voice server updated.");

    return Task.CompletedTask;
  }

  private Task AudioServiceOnWebSocketClosed(object sender, WebSocketClosedEventArgs eventArgs)
  {
    Logger.LogCritical("Discord websocket has closed for the following reason: {Reason}", eventArgs.Reason);

    return Task.CompletedTask;
  }

  private Task AudioServiceOnTrackStuck(object sender, TrackStuckEventArgs eventArgs)
  {
    Logger.LogInformation("Track {TrackName} is stuck while trying to play.", eventArgs.Track.Identifier);

    return Task.CompletedTask;
  }

  private async Task AudioServiceOnTrackStarted(object sender, TrackStartedEventArgs eventArgs)
  {
    // Get text channel and send message to it
    var player = (HowbotPlayer)eventArgs.Player;
    var track = eventArgs.Track;
    var channel = player.TextChannel;

    Logger.LogDebug("Starting track [{TrackTitle}]", eventArgs.Track.Title);

    await channel.SendMessageAsync(embed: embedService.CreateNowPlayingEmbed(new ExtendedLavalinkTrack(track)) as Embed);
  }

  private Task AudioServiceOnTrackException(object sender, TrackExceptionEventArgs eventArgs)
  {
    var exception = new Exception(eventArgs.Exception.Message);

    Logger.LogError(exception, "[{Severity}] - LavaNode has thrown an exception",
      eventArgs.Exception.Severity);

    return Task.CompletedTask;
  }

  private Task AudioServiceOnTrackEnded(object sender, TrackEndedEventArgs eventArgs)
  {
    if (eventArgs.Reason is not TrackEndReason.Finished)
    {
      Logger.LogInformation("Track [{TrackName}] has ended with reason [{Reason}]", eventArgs.Track.Title,
        eventArgs.Reason);
      return Task.CompletedTask;
    }
    
    Logger.LogDebug("Current song [{SongName}] has ended.", eventArgs.Track.Title);

    return Task.CompletedTask;
  }

  private Task AudioServiceOnStatisticsUpdated(object sender, StatisticsUpdatedEventArgs eventArgs)
  {
    Logger.LogDebug(eventArgs.Statistics.ToString());

    return Task.CompletedTask;
  }

  #endregion Events

  public async ValueTask DisposeAsync()
  {
    await audioService.DisposeAsync();

    audioService.StatisticsUpdated -= AudioServiceOnStatisticsUpdated;
    audioService.TrackEnded -= AudioServiceOnTrackEnded;
    audioService.TrackException -= AudioServiceOnTrackException;
    audioService.TrackStarted -= AudioServiceOnTrackStarted;
    audioService.TrackStuck -= AudioServiceOnTrackStuck;
    audioService.WebSocketClosed -= AudioServiceOnWebSocketClosed;

    GC.SuppressFinalize(this);
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
