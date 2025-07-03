using Howbot.Application.Interfaces.Discord;
using Howbot.SharedKernel;

namespace Howbot.Worker.Discord;

public class Worker(ILoggerAdapter<Worker> logger, IHowbotService howbotService) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      if (logger.IsLogLevelEnabled(LogLevel.Debug))
      {
        logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
      }
      await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
  }

  public override async Task StartAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    logger.LogDebug("Starting the Discord bot worker...");

    await howbotService.StartWorkerServiceAsync(cancellationToken);

    await base.StartAsync(cancellationToken);
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    logger.LogDebug("Stopping the Discord bot worker...");

    await howbotService.StopWorkerServiceAsync(cancellationToken);

    await base.StopAsync(cancellationToken);
  }
}
