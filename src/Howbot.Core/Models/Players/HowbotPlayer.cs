using Discord;
using Lavalink4NET.InactivityTracking.Players;
using Lavalink4NET.InactivityTracking.Trackers;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Models.Players;

public class HowbotPlayer(IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties)
  : QueuedLavalinkPlayer(properties), IInactivityPlayerListener
{
  private readonly ILogger<HowbotPlayer> _logger = properties.Logger;
  public ITextChannel? TextChannel { get; } = properties.Options.Value.TextChannel;
  public bool IsAutoPlayEnabled { get; set; } = properties.Options.Value.IsAutoPlayEnabled;

  #region Inactivity Tracking Events

  public ValueTask NotifyPlayerActiveAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    _logger.LogDebug("Player is being tracked as active");
    
    return ValueTask.CompletedTask;
  }

  public ValueTask NotifyPlayerInactiveAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    _logger.LogDebug("Player exceeded inactive timeout");

    return ValueTask.CompletedTask;
  }

  public ValueTask NotifyPlayerTrackedAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    _logger.LogDebug("Player is being tracked as inactive");

    return ValueTask.CompletedTask;
  }

  #endregion Inactivity Tracking Events
}
