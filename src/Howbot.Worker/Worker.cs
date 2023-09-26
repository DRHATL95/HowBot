﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Exceptions;
using Howbot.Core.Settings;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Howbot.Worker;

public class Worker : BackgroundService
{
  [NotNull] private readonly IDiscordClientService _discordClientService;
  [NotNull] private readonly ILoggerAdapter<Worker> _logger;
  [NotNull] private readonly IServiceProvider _serviceProvider;
  [NotNull] private readonly DiscordSocketClient _discordSocketClient;

  public Worker([NotNull] IDiscordClientService discordClientService, [NotNull] IServiceProvider serviceProvider, DiscordSocketClient discordSocketClient, [NotNull] ILoggerAdapter<Worker> logger)
  {
    _discordClientService = discordClientService;
    _serviceProvider = serviceProvider;
    _discordSocketClient = discordSocketClient;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    try
    {
      InitializeHowbotServices(cancellationToken);

      if (!await _discordClientService.LoginDiscordBotAsync(Configuration.DiscordToken).ConfigureAwait(false))
      {
        _logger.LogCritical(
          "Unable to login to discord with provided token."); // New exception type? (DiscordLoginException)

        // Stop worker, cannot continue without being authenticated
        await StopAsync(cancellationToken);

        throw new DiscordLoginException("Unable to login to discord API with token.");
      }

      await _discordClientService.StartDiscordBotAsync().ConfigureAwait(false);

      // Run worker service indefinitely until cancellationToken is created or process is manually stopped.
      await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
    catch (DiscordLoginException loginException)
    {
      _logger.LogError(loginException, "An exception has been thrown logging into Discord.");
      throw;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "An exception has been thrown in the main worker");
      throw;
    }
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    await base.StopAsync(cancellationToken);

    await _discordSocketClient.StopAsync().ConfigureAwait(false);

    _logger.LogInformation("Discord client has been stopped.");
  }

  private void InitializeHowbotServices(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    _logger.LogDebug("Starting initialization of Howbot services");

    try
    {
      // Call each service 'initialize' function primarily used for hooking up events
      using var scope = _serviceProvider.CreateScope();

      scope.ServiceProvider.GetRequiredService<IDiscordClientService>()?.Initialize();
      scope.ServiceProvider.GetRequiredService<IInteractionHandlerService>()?.Initialize();

      /*_serviceProvider.GetService<IDiscordClientService>().Initialize();
      _serviceProvider.GetService<IInteractionHandlerService>().Initialize();
      // _serviceProvider.GetService<IDeploymentService>().Initialize();
      // _serviceProvider.GetService<IDockerService>().Initialize();
      _serviceProvider.GetService<IEmbedService>().Initialize();
      _serviceProvider.GetService<IMusicService>().Initialize();
      _serviceProvider.GetService<IVoiceService>().Initialize();
      _serviceProvider.GetService<ILavaNodeService>().Initialize();*/
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }
  
}
