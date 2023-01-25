using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

namespace Howbot.Core.Services;

public class LavaNodeService : ILavaNodeService
{
  private readonly LavaNode _lavaNode;
  private readonly IEmbedService _embedService;
  private readonly ILoggerAdapter<LavaNodeService> _logger;
  private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;

  public LavaNodeService(LavaNode lavaNode, IEmbedService embedService, ILoggerAdapter<LavaNodeService> logger)
  {
    _lavaNode = lavaNode;
    _embedService = embedService;
    _logger = logger;
    _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
  }
  
  public void Initialize()
  {
    if (_lavaNode == null) return;
    
    // Hook-up lava node events
    _lavaNode.OnTrackStart += LavaNodeOnOnTrackStart;
    _lavaNode.OnTrackEnd += LavaNodeOnOnTrackEnd;
    _lavaNode.OnTrackException += LavaNodeOnOnTrackException;
    _lavaNode.OnStatsReceived += LavaNodeOnOnStatsReceived;
    _lavaNode.OnWebSocketClosed += LavaNodeOnOnWebSocketClosed;
    _lavaNode.OnTrackStuck += LavaNodeOnOnTrackStuck;
  }

  #region Lava Node Events

  private Task LavaNodeOnOnTrackStuck(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackStuckEventArg)
  {
    var guild = GuildHelper.GetGuildTag(trackStuckEventArg.Player.TextChannel.Guild);
    
    _logger.LogInformation("Requested track [{TrackTitle}] for Guild {GuildTag} is stuck", 
      trackStuckEventArg.Track.Title, guild);

    return Task.CompletedTask;
  }

  private Task LavaNodeOnOnWebSocketClosed(WebSocketClosedEventArg arg)
  {
    _logger.LogInformation("Discord websocket has closed for the following reason: {Reason}", arg.Reason);

    return Task.CompletedTask;
  }

  private Task LavaNodeOnOnStatsReceived(StatsEventArg arg)
  {
    StringBuilder stringBuilder = new StringBuilder();

    stringBuilder.AppendLine($"Lavalink has been online for: {arg.Uptime}");
    stringBuilder.AppendLine($"CPU Cores: {arg.Cpu.Cores}");
    stringBuilder.AppendLine($"Total players playing: {arg.PlayingPlayers}");

    var message = stringBuilder.ToString();
    
    _logger.LogDebug(message);

    return Task.CompletedTask;
  }

  private Task LavaNodeOnOnTrackException(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackExceptionEventArg)
  {
    var exception = new Exception(trackExceptionEventArg.Exception.Message);
    
    _logger.LogError(exception, "[{Severity}] - LavaNode has thrown an exception", trackExceptionEventArg.Exception.Severity);

    return Task.CompletedTask;
  }

  private async Task LavaNodeOnOnTrackEnd(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackEndEventArg)
  {
    if (trackEndEventArg.Reason is not TrackEndReason.Finished)
      return;

    var lavaPlayer = trackEndEventArg.Player;
    if (lavaPlayer != null)
    {
      if (!lavaPlayer.Vueue.TryDequeue(out var lavaTrack))
      {
        _logger.LogInformation("Lava player queue is empty. Attempting to disconnect now");
        await this.InitiateDisconnectLogicAsync(lavaPlayer as LavaPlayer, TimeSpan.FromSeconds(30));
        return;
      }
      
      _logger.LogDebug("Track has ended. Playing next song in queue.");
      await lavaPlayer.PlayAsync(lavaTrack);
    }
  }

  private Task LavaNodeOnOnTrackStart(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackStartEventArg)
  {
    var guild = trackStartEventArg.Player.TextChannel.Guild;
    
    _logger.LogDebug("Track [{TrackName}] has started playing in Guild {GuildTag}", 
      trackStartEventArg.Track.Title, GuildHelper.GetGuildTag(guild));

    return Task.CompletedTask;
  }

  #endregion

  private async Task InitiateDisconnectLogicAsync(LavaPlayer lavaPlayer, TimeSpan timeSpan)
  {
    if (_disconnectTokens.TryGetValue(lavaPlayer.VoiceChannel.Id, out var value))
    {
      value = new CancellationTokenSource();
      _disconnectTokens.TryAdd(lavaPlayer.VoiceChannel.Id, value);
    }
    else if (value.IsCancellationRequested)
    {
      _disconnectTokens.TryUpdate(lavaPlayer.VoiceChannel.Id, new CancellationTokenSource(), value);
      value = _disconnectTokens[lavaPlayer.VoiceChannel.Id];
    }
    
    _logger.LogInformation("Disconnecting from voice channel in {Seconds}", timeSpan.Seconds);

    var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
    if (isCancelled) return;

    await _lavaNode.LeaveAsync(lavaPlayer.VoiceChannel);
    _logger.LogDebug("Howbot has disconnected from voice channel.");
  }
}
