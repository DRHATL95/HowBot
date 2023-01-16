using System;
using System.Text;
using System.Threading.Tasks;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

namespace Howbot.Core.Services;

public class LavaNodeService : ILavaNodeService
{
  private readonly ILoggerAdapter<LavaNodeService> _logger;

  public LavaNodeService(ILoggerAdapter<LavaNodeService> logger, LavaNode lavaNode)
  {
    _logger = logger;
    
    // Hook-up lava node events
    lavaNode.OnTrackStart += LavaNodeOnOnTrackStart;
    lavaNode.OnTrackEnd += LavaNodeOnOnTrackEnd;
    lavaNode.OnTrackException += LavaNodeOnOnTrackException;
    lavaNode.OnStatsReceived += LavaNodeOnOnStatsReceived;
    lavaNode.OnWebSocketClosed += LavaNodeOnOnWebSocketClosed;
    lavaNode.OnTrackStuck += LavaNodeOnOnTrackStuck;
  }

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

    stringBuilder.AppendLine();
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

  private Task LavaNodeOnOnTrackEnd(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackEndEventArg)
  {
    var guild = trackEndEventArg.Player.TextChannel.Guild;
    
    _logger.LogDebug("Track [{TrackName}] has stopped playing in Guild {GuildTag}", 
      trackEndEventArg.Track.Title, GuildHelper.GetGuildTag(guild));

    return Task.CompletedTask;
  }

  private Task LavaNodeOnOnTrackStart(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> trackStartEventArg)
  {
    var guild = trackStartEventArg.Player.TextChannel.Guild;
    
    _logger.LogDebug("Track [{TrackName}] has started playing in Guild {GuildTag}", 
      trackStartEventArg.Track.Title, GuildHelper.GetGuildTag(guild));

    return Task.CompletedTask;
  }
}
