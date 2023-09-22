using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Serilog;

namespace Howbot.Core.Models.Players;

public class HowbotPlayer : QueuedLavalinkPlayer
{

  public HowbotPlayer([NotNull] IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties) : base(properties)
  {

  }

  protected override async ValueTask NotifyTrackStartedAsync(ITrackQueueItem queueItem,
    CancellationToken cancellationToken = new CancellationToken())
  {
    await base.NotifyTrackStartedAsync(queueItem, cancellationToken);

    Log.Logger.Information("Track started: {0}", queueItem.Track?.Title);
  }
}
