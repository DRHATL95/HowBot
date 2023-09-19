using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using JetBrains.Annotations;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;

namespace Howbot.Core.Models.Players;

public class HowbotQueuedPlayer : QueuedLavalinkPlayer
{
  private readonly ILoggerAdapter<HowbotQueuedPlayer> _logger;

  public HowbotQueuedPlayer([NotNull] IPlayerProperties<HowbotQueuedPlayer, HowbotQueuedPlayerOptions> properties, ILoggerAdapter<HowbotQueuedPlayer> logger) : base(properties)
  {
    _logger = logger;
  }

  protected override async ValueTask NotifyTrackStartedAsync(ITrackQueueItem queueItem,
    CancellationToken cancellationToken = new CancellationToken())
  {
    await base.NotifyTrackStartedAsync(queueItem, cancellationToken);

    _logger.LogInformation("Track started: {0}", queueItem.Track?.Title);
  }
}
