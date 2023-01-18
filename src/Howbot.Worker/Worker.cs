using System;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Howbot.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
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
  private readonly IServiceLocator _serviceLocator;
  // private readonly WorkerSettings _settings;

  public Worker(ILoggerAdapter<Worker> logger, IServiceLocator serviceLocator)
  {
    _logger = logger;
    _serviceLocator = serviceLocator;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Worker service starting..");

    using var scope = _serviceLocator.CreateScope();
    var discordClientService = scope.ServiceProvider.GetRequiredService<IDiscordClientService>();
    var configuration = scope.ServiceProvider.GetRequiredService<Configuration>();

    if (!(await discordClientService.LoginDiscordBotAsync(configuration.DiscordToken)))
    {
      _logger.LogCritical("Unable to login to discord with provided token."); // New exception type? (DiscordLoginException)
      await this.StopAsync(cancellationToken); // Stop worker, cannot continue without being authenticated
    }

    await discordClientService.StartDiscordBotAsync();

    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    
    _logger.LogInformation("Worker service stopped!");
  }
}
