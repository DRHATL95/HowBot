using System.Threading;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Settings;
using Lavalink4NET.InactivityTracking.Players;
using Lavalink4NET.InactivityTracking.Trackers;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Protocol.Payloads.Events;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Models.Players;

public class HowbotPlayer(IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties)
  : QueuedLavalinkPlayer(properties), IInactivityPlayerListener
{
  private readonly ILogger<HowbotPlayer> _logger = properties.Logger;
  public bool IsTwoFourSevenEnabled { get; set; } = true; // Change back on release to false
  public ITextChannel TextChannel { get; } = properties.Options.Value.TextChannel;

  protected override async ValueTask NotifyTrackEndedAsync(ITrackQueueItem queueItem, TrackEndReason endReason,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    if (!IsTwoFourSevenEnabled)
    {
      await base.NotifyTrackEndedAsync(queueItem, endReason, cancellationToken);
      return;
    }

    _logger.LogDebug("Track ended, but player is in 24/7 mode");

    // Add track to history
    if (Queue.History is not null && Queue.HasHistory)
    {
      await Queue.History
        .AddAsync(queueItem, cancellationToken)
        .ConfigureAwait(false);
    }
  }

  #region Inactivity Tracking Events

  public async ValueTask NotifyPlayerActiveAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    _logger.LogDebug("Player is being tracked as active");

    if (TextChannel is not null && Configuration.IsDebug())
    {
      await TextChannel.SendMessageAsync("Player is being tracked as active");
    }
  }

  public ValueTask NotifyPlayerInactiveAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    _logger.LogDebug("Player exceeded inactive timeout");

    return ValueTask.CompletedTask;
  }

  public async ValueTask NotifyPlayerTrackedAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    _logger.LogDebug("Player is being tracked as inactive");

    if (TextChannel is not null && Configuration.IsDebug())
    {
      await TextChannel.SendMessageAsync("Player is being tracked as inactive");
    }
  }

  #endregion
}
