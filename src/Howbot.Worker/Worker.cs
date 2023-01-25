using System;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Howbot.Core.Interfaces;
using Howbot.Core.Services;
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
  private readonly IDiscordClientService _discordClientService;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILoggerAdapter<Worker> _logger;
  
  public Worker(IDiscordClientService discordClientService, IServiceProvider serviceProvider, ILoggerAdapter<Worker> logger)
  {
    _discordClientService = discordClientService;
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation("Worker service starting..");
    
      _logger.LogDebug("Initializing howbot services..");
    
      InitializeHowbotServices();
    
      if (!(await _discordClientService.LoginDiscordBotAsync(Configuration.DiscordToken)))
      {
        _logger.LogCritical("Unable to login to discord with provided token."); // New exception type? (DiscordLoginException)
        
        await this.StopAsync(cancellationToken); // Stop worker, cannot continue without being authenticated
        
        _logger.LogInformation("Worker service stopped!");
        
        return;
      }
      
      await _discordClientService.StartDiscordBotAsync();

      // Run worker service indefinitely until cancellationToken is created or process is manually stopped.
      await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    
      _logger.LogInformation("Worker service stopped!");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception,"An exception has been thrown in the main worker");
    }
  }

  private void InitializeHowbotServices()
  {
    if (_serviceProvider == null) return;
    
    try
    {
      _serviceProvider.GetService<IDiscordClientService>().Initialize();
      _serviceProvider.GetService<IInteractionHandlerService>().Initialize();
      _serviceProvider.GetService<IDeploymentService>().Initialize();
      _serviceProvider.GetService<IDockerService>().Initialize();
      _serviceProvider.GetService<IEmbedService>().Initialize();
      _serviceProvider.GetService<ILavaNodeService>().Initialize();
      _serviceProvider.GetService<IMusicService>().Initialize();
      _serviceProvider.GetService<IVoiceService>().Initialize();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }
}
