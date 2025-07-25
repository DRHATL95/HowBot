using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Howbot.Infrastructure.Services;
public class NotificationBackgroundService : BackgroundService
{
  private readonly INotificationService _notificationService;
  private readonly ILogger<NotificationBackgroundService> _logger;

  public NotificationBackgroundService(
      INotificationService notificationService,
      ILogger<NotificationBackgroundService> logger)
  {
    _notificationService = notificationService;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Notification background service starting");

    // Ensure SignalR connection is established
    if (_notificationService is HybridNotificationService notificationService)
    {
      await notificationService.EnsureConnectionAsync();
    }

    // Keep the service running
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        // Periodic health check or queue processing could go here
        _logger.LogDebug("Notification service heartbeat");
      }
      catch (OperationCanceledException)
      {
        break;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error in notification background service");
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
      }
    }

    _logger.LogInformation("Notification background service stopping");
  }
}
