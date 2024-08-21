using System;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Howbot.Worker;

public class Worker(IHowbotService howbotService, ILoggerAdapter<Worker> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    try
    {
      logger.LogDebug("Executing the main worker service. Listening for commands from the API.");

      // Since our worker isn't doing much, all we care about is just waiting for the cancellation token to be triggered.
      await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
    catch (Exception exception)
    {
      logger.LogCritical(exception, "An exception has been thrown in the main worker");
    }
  }

  public override async Task StartAsync(CancellationToken cancellationToken)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      logger.LogDebug("Starting the worker service");

      await howbotService.StartWorkerServiceAsync(cancellationToken);

      await base.StartAsync(cancellationToken);
    }
    catch (Exception exception)
    {
      logger.LogCritical(exception,
        "A critical exception has been thrown in the main worker. Stopping the worker service");
    }
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      await base.StopAsync(cancellationToken);

      await howbotService.StopWorkerServiceAsync(cancellationToken);
    }
    catch (Exception exception)
    {
      logger.LogCritical(exception,
        "A critical exception has been thrown in the main worker. Stopping the worker service");
    }
  }
}
