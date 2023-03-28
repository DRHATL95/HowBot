using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Responses.Search;

namespace Howbot.Core.Services;

public class LavaNodeService : ServiceBase<LavaNodeService>, ILavaNodeService
{
  private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
  private readonly IEmbedService _embedService;
  private readonly LavaNode<Player<LavaTrack>, LavaTrack> _lavaNode;
  private readonly ILoggerAdapter<LavaNodeService> _logger;
  private readonly IMusicService _musicService;

  public LavaNodeService(LavaNode<Player<LavaTrack>, LavaTrack> lavaNode, IEmbedService embedService,
    IMusicService musicService, ILoggerAdapter<LavaNodeService> logger) : base(logger)
  {
    _lavaNode = lavaNode;
    _embedService = embedService;
    _logger = logger;
    _musicService = musicService;
    _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
  }

  public new void Initialize()
  {
    if (_lavaNode == null)
    {
      return;
    }

    if (_logger.IsLogLevelEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("{ServiceName} is now initializing..", nameof(LavaNodeService));
    }

    // Hook-up lava node events
    _lavaNode.OnTrackStart += LavaNodeOnOnTrackStart;
    _lavaNode.OnTrackEnd += LavaNodeOnOnTrackEnd;
    _lavaNode.OnTrackException += LavaNodeOnOnTrackException;
    _lavaNode.OnStatsReceived += LavaNodeOnOnStatsReceived;
    _lavaNode.OnWebSocketClosed += LavaNodeOnOnWebSocketClosed;
    _lavaNode.OnTrackStuck += LavaNodeOnOnTrackStuck;
  }

  public async Task InitiateDisconnectLogicAsync(Player<LavaTrack> lavaPlayer, TimeSpan timeSpan)
  {
    if (!_disconnectTokens.TryGetValue(lavaPlayer.VoiceChannel.Id, out var value))
    {
      value = new CancellationTokenSource();
      _disconnectTokens.TryAdd(lavaPlayer.VoiceChannel.Id, value);
    }
    else if (value.IsCancellationRequested)
    {
      _disconnectTokens.TryUpdate(lavaPlayer.VoiceChannel.Id, new CancellationTokenSource(), value);
      value = _disconnectTokens[lavaPlayer.VoiceChannel.Id];
    }

    await lavaPlayer.TextChannel.SendMessageAsync($"Auto disconnect initiated! Disconnecting in {timeSpan}...");
    var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
    if (isCancelled)
    {
      return;
    }

    await _lavaNode.LeaveAsync(lavaPlayer.VoiceChannel);
    _logger.LogDebug("Howbot has disconnected from voice channel.");
  }

  private async Task PlayRadioTrack(Player<LavaTrack> lavaPlayer)
  {
    ArgumentNullException.ThrowIfNull(lavaPlayer);

    if (lavaPlayer.LastPlayed != null)
    {
      if (await _musicService.GetYoutubeRecommendedVideoId(lavaPlayer.LastPlayed.Id, Constants.RadioSearchLength) is
            List<string> result && result.Any())
      {
        var videoId = result.First();
        var videoUrl = Constants.YouTubeBaseShortUrl + videoId;

        var searchResult = await _lavaNode.SearchAsync(SearchType.Direct, videoUrl);
        if (searchResult.Tracks.Any())
        {
          // Because the player end event doesn't fire when calling play we need to set last played here
          lavaPlayer.LastPlayed = lavaPlayer.Track;
          lavaPlayer.RecentlyPlayed.Add(lavaPlayer.LastPlayed);

          var nextTrack = searchResult.Tracks.First();

          // _logger.LogDebug("Last Track: [{TrackOne}] | Next Track: [{TrackTwo}]", lavaPlayer.LastPlayed, nextTrack);

          await lavaPlayer.PlayAsync(nextTrack);
        }
      }
    }
  }

  private void GetUniqueRadioTrack(List<LavaTrack> recentTracks, LavaTrack requestedTrack)
  {
    ArgumentNullException.ThrowIfNull(recentTracks);
    ArgumentNullException.ThrowIfNull(requestedTrack);
  }

  #region Lava Node Events

  /// <summary>
  ///   LavaNode event handler for when a track gets stuck.
  /// </summary>
  /// <param name="trackStuckEventArg"></param>
  /// <returns></returns>
  private Task LavaNodeOnOnTrackStuck(TrackStuckEventArg<Player<LavaTrack>, LavaTrack> trackStuckEventArg)
  {
    var guild = GuildHelper.GetGuildTag(trackStuckEventArg.Player.TextChannel.Guild);

    _logger.LogWarning("Requested track [{TrackTitle}] for Guild {GuildTag} is stuck",
      trackStuckEventArg.Track.Title, guild);

    return Task.CompletedTask;
  }

  /// <summary>
  ///   LavaNode websocket event handler for when the socket is closed unexpectedly.
  /// </summary>
  /// <param name="arg"></param>
  /// <returns></returns>
  private Task LavaNodeOnOnWebSocketClosed(WebSocketClosedEventArg arg)
  {
    _logger.Log(LogLevel.Critical, "Discord websocket has closed for the following reason: {Reason}", arg.Reason);

    return Task.CompletedTask;
  }

  /// <summary>
  ///   LavaNode event handler for when stats are received from the server.
  /// </summary>
  /// <param name="arg"></param>
  /// <returns></returns>
  private Task LavaNodeOnOnStatsReceived(StatsEventArg arg)
  {
    _logger.LogDebug($"Lavalink has been online for: {arg.Uptime:g}");

    return Task.CompletedTask;
  }

  /// <summary>
  ///   LavaNode event handler for when an exception has been thrown.
  /// </summary>
  /// <param name="trackExceptionEventArg"></param>
  /// <returns></returns>
  private Task LavaNodeOnOnTrackException(TrackExceptionEventArg<Player<LavaTrack>, LavaTrack> trackExceptionEventArg)
  {
    var exception = new Exception(trackExceptionEventArg.Exception.Message);

    _logger.LogError(exception, "[{Severity}] - LavaNode has thrown an exception",
      trackExceptionEventArg.Exception.Severity);

    return Task.CompletedTask;
  }

  /// <summary>
  ///   LavaNode event handler for when track ends.
  ///   TODO: in the future, will check for radio mode set
  /// </summary>
  /// <param name="trackEndEventArg"></param>
  private async Task LavaNodeOnOnTrackEnd(TrackEndEventArg<Player<LavaTrack>, LavaTrack> trackEndEventArg)
  {
    if (trackEndEventArg.Reason is not TrackEndReason.Finished)
    {
      return;
    }

    var lavaPlayer = trackEndEventArg.Player;
    if (lavaPlayer != null)
    {
      // set last played track
      lavaPlayer.LastPlayed ??= trackEndEventArg.Track;

      if (!lavaPlayer.Vueue.TryDequeue(out var lavaTrack))
      {
        _logger.LogInformation("Player queue is empty but we are in radio mode!");
        if (trackEndEventArg.Player.IsRadioMode)
        {
          await PlayRadioTrack(lavaPlayer);
          return;
        }

        _logger.LogInformation("Lava player queue is empty. Attempting to disconnect now");
        await InitiateDisconnectLogicAsync(lavaPlayer, TimeSpan.FromSeconds(30));
        return;
      }

      _logger.LogDebug("Track has ended. Playing next song in queue.");
      await lavaPlayer.PlayAsync(lavaTrack);
    }
  }

  /// <summary>
  ///   LavaNode event handler for when track starts
  /// </summary>
  /// <param name="trackStartEventArg"></param>
  /// <returns></returns>
  private async Task LavaNodeOnOnTrackStart(TrackStartEventArg<Player<LavaTrack>, LavaTrack> trackStartEventArg)
  {
    var guild = trackStartEventArg.Player.TextChannel.Guild;
    var textChannel = trackStartEventArg.Player.TextChannel;

    _logger.LogDebug("Track [{TrackName}] has started playing in Guild {GuildTag}",
      trackStartEventArg.Track.Title, GuildHelper.GetGuildTag(guild));

    var embed = await _embedService.GenerateMusicNowPlayingEmbedAsync(trackStartEventArg.Track,
      trackStartEventArg.Player.Author, textChannel);
    await textChannel.SendMessageAsync(embed: embed as Embed);
  }

  #endregion
}
