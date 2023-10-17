using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lavalink4NET.InactivityTracking.Players;
using Lavalink4NET.InactivityTracking.Trackers;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Serilog;

namespace Howbot.Core.Models.Players;

public class HowbotPlayer : QueuedLavalinkPlayer, IInactivityPlayerListener
{
  public HowbotPlayer([NotNull] IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties) : base(properties)
  {
  }

  public bool IsTwoFourSevenEnabled { get; set; } = false;

  public ValueTask NotifyPlayerActiveAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    Log.Debug("Player is being tracked as active.");

    return ValueTask.CompletedTask;
  }

  public ValueTask NotifyPlayerInactiveAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    Log.Debug("Player exceeded inactive timeout.");

    return ValueTask.CompletedTask;
  }

  public ValueTask NotifyPlayerTrackedAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    Log.Debug("Player is being tracked as inactive.");

    return ValueTask.CompletedTask;
  }

  protected override async ValueTask NotifyTrackStartedAsync(ITrackQueueItem queueItem,
    CancellationToken cancellationToken = new CancellationToken())
  {
    await base.NotifyTrackStartedAsync(queueItem, cancellationToken);

    Log.Logger.Information("Track started: {0}", queueItem.Track?.Title);
  }
}
