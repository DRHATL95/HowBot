using System.Threading;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Settings;
using Lavalink4NET.InactivityTracking.Players;
using Lavalink4NET.InactivityTracking.Trackers;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Microsoft.Extensions.Logging;

namespace Howbot.Core.Models.Players;

public class HowbotPlayer(IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties)
  : QueuedLavalinkPlayer(properties), IInactivityPlayerListener
{
  // public bool IsTwoFourSevenEnabled { get; set; } = false;
  private readonly ILogger<HowbotPlayer> _logger = properties.Logger;
  public ITextChannel TextChannel { get; } = properties.Options.Value.TextChannel;
  public IUser User { get; } = properties.Options.Value.LastRequestedBy;

  public async ValueTask NotifyPlayerActiveAsync(PlayerTrackingState trackingState,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    
    _logger.LogDebug("Player is being tracked as active");
    
    if (TextChannel is not null && Configuration.IsDebug())
    {
      await TextChannel.SendMessageAsync("Player is being tracked as active").ConfigureAwait(false);
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
      await TextChannel.SendMessageAsync("Player is being tracked as inactive").ConfigureAwait(false);
    }
  }
}
