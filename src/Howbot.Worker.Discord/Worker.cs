using Howbot.Application.Interfaces.Discord;
using Howbot.SharedKernel;

namespace Howbot.Worker.Discord;

public class Worker(ILoggerAdapter<Worker> logger, IHowbotService howbotService) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
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
    catch (OperationCanceledException)
    {
      // This exception is expected when the service is stopped.
      logger.LogDebug("Worker execution was cancelled.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An error occurred while executing the worker.");
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
