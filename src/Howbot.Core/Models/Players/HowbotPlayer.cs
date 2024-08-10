using Discord;
using Howbot.Core.Interfaces;
using Lavalink4NET.InactivityTracking.Players;
using Lavalink4NET.InactivityTracking.Trackers;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;

namespace Howbot.Core.Models.Players;

public class HowbotPlayer(IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties, ILoggerAdapter<HowbotPlayer> logger)
  : QueuedLavalinkPlayer(properties), IInactivityPlayerListener
{
  public ITextChannel? TextChannel { get; } = properties.Options.Value.TextChannel;
  public bool IsAutoPlayEnabled { get; set; } = properties.Options.Value.IsAutoPlayEnabled;
  public ITrackQueue AutoPlayQueue { get; } = new TrackQueue();
  
  #region Inactivity Tracking Events

  public ValueTask NotifyPlayerActiveAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    
    logger.LogDebug("Player is being tracked as active");
    
    return ValueTask.CompletedTask;
  }

  public ValueTask NotifyPlayerInactiveAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    logger.LogDebug("Player exceeded inactive timeout");

    return ValueTask.CompletedTask;
  }

  public ValueTask NotifyPlayerTrackedAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    logger.LogDebug("Player is being tracked as inactive");

    return ValueTask.CompletedTask;
  }

  #endregion Inactivity Tracking Events
}
