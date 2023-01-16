using System;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Howbot.Worker;

/// <summary>
/// The Worker is a BackgroundService -
/// It should not contain any business logic but should call an entrypoint service that
/// execute once.
/// </summary>
public class Worker : BackgroundService
{
  private readonly ILoggerAdapter<Worker> _logger;
  private readonly IEntryPointService _entryPointService;
  private readonly WorkerSettings _settings;

  public Worker(ILoggerAdapter<Worker> logger,
      IEntryPointService entryPointService,
      WorkerSettings settings)
  {
    _logger = logger;
    _entryPointService = entryPointService;
    _settings = settings;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Worker service starting..", DateTimeOffset.Now);

    await _entryPointService.ExecuteAsync(stoppingToken);
    
    _logger.LogInformation("Worker service stopped!", DateTimeOffset.Now);
  }
}
