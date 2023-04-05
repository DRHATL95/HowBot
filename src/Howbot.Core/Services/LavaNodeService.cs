using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Responses.Search;
using static System.Threading.SpinWait;

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
    if (_logger.IsLogLevelEnabled(LogLevel.Debug))
    {
      _logger.LogDebug("{ServiceName} is now initializing..", nameof(LavaNodeService));
    }
    
    if (_lavaNode == null)
    {
      return;
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
    var isCancelled = SpinUntil(() => value.IsCancellationRequested, timeSpan);
    if (isCancelled)
    {
      _logger.LogDebug("Auto disconnect cancelled.");
      return;
    }

    await _lavaNode.LeaveAsync(lavaPlayer.VoiceChannel);
    _logger.LogDebug("Howbot has disconnected from voice channel.");
  }

  private async Task Play247Track(Player<LavaTrack> lavaPlayer)
  {
    ArgumentNullException.ThrowIfNull(lavaPlayer);

    try
    {
      if (lavaPlayer.LastPlayed != null)
      {
        var track = await this.GetUniqueRadioTrack(lavaPlayer);
        if (track != null)
        {
          await lavaPlayer.PlayAsync(track);
        }
        else
        {
          _logger.LogDebug("Unable to find a track to play.");
        }
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }

  [ItemCanBeNull]
  // ReSharper disable once CognitiveComplexity
  private async Task<LavaTrack> GetUniqueRadioTrack(Player<LavaTrack> player, /*List<string> videoIds = null,*/ int attempt = 0)
  {
    ArgumentNullException.ThrowIfNull(player);
    
    // Recursive base case
    if (attempt >= Constants.MaximumUniqueSearchAttempts)
    {
      _logger.LogDebug("Unable to find a song after {SearchLimit} attempts.", Constants.MaximumUniqueSearchAttempts);
      return null;
    }
    
    if (player.LastPlayed == null && !player.RecentlyPlayed.Any())
    {
      _logger.LogInformation("Unable to find another song, last song is null.");
      return null;
    }

    List<string> uniqueVideoIds;
    
    if (attempt > 0)
    {
      // Recursive pass
      uniqueVideoIds = (await _musicService.GetYoutubeRecommendedVideoId(player.LastPlayed!.Id, Constants.RelatedSearchResultsLimit + attempt)).ToList();
    }
    else
    {
      // First pass
      uniqueVideoIds = (await _musicService.GetYoutubeRecommendedVideoId(player.LastPlayed!.Id, Constants.RelatedSearchResultsLimit)).ToList();
    }
    
    var videoId = uniqueVideoIds.FirstOrDefault(x => player.RecentlyPlayed.Any(y => y.Id != x));
    if (string.IsNullOrEmpty(videoId))
    {
      // Recursive call
      return await this.GetUniqueRadioTrack(player, ++attempt);
    }
    
    var videoUrl = Constants.YouTubeBaseShortUrl + videoId;
    var searchResult = await _lavaNode.SearchAsync(SearchType.Direct, videoUrl);
    if (!searchResult.Tracks.Any())
    {
      return null;
    }
    
    var nextTrack = searchResult.Tracks.First();
    if (player.RecentlyPlayed.Any(x => x.Id == nextTrack.Id))
    {
      // Recursive call
      return await this.GetUniqueRadioTrack(player, ++attempt);
    }
    
    if (MusicHelper.AreTracksSimilar(player.LastPlayed, nextTrack))
    {
      // Recursive call
      return await this.GetUniqueRadioTrack(player, ++attempt);
    }

    return nextTrack;
    
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
      lavaPlayer.RecentlyPlayed.Add(lavaPlayer.LastPlayed);

      if (!lavaPlayer.Vueue.TryDequeue(out var lavaTrack))
      {
        if (trackEndEventArg.Player.Is247ModeEnabled)
        {
          _logger.LogInformation("Player queue is empty, but 24/7 mode is enabled. Playing next related track.");
          
          await Play247Track(lavaPlayer);
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
