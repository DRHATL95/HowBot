using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Exceptions;
using Howbot.Core.Settings;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Howbot.Worker;

public class Worker : BackgroundService
{
  [NotNull] private readonly IDiscordClientService _discordClientService;
  [NotNull] private readonly DiscordSocketClient _discordSocketClient;
  [NotNull] private readonly ILoggerAdapter<Worker> _logger;
  [NotNull] private readonly IServiceProvider _serviceProvider;

  public Worker([NotNull] IDiscordClientService discordClientService, [NotNull] IServiceProvider serviceProvider,
    [NotNull] DiscordSocketClient discordSocketClient, [NotNull] ILoggerAdapter<Worker> logger)
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
      cancellationToken.ThrowIfCancellationRequested();

      InitializeHowbotServices(cancellationToken);

      if (!await _discordClientService.LoginDiscordBotAsync(Configuration.DiscordToken).ConfigureAwait(false))
      {
        _logger.LogCritical("Unable to login to discord API with token.");

        // Stop worker, cannot continue without being authenticated
        await StopAsync(cancellationToken).ConfigureAwait(false);

        throw new DiscordLoginException("Unable to login to discord API with token.");
      }

      await _discordClientService.StartDiscordBotAsync().ConfigureAwait(false);

      // Run worker service indefinitely until cancellationToken is created or process is manually stopped.
      await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
    catch (DiscordLoginException loginException)
    {
      _logger.LogError(loginException, "An exception has been thrown logging into Discord.", Array.Empty<object>());
      throw;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "An exception has been thrown in the main worker", Array.Empty<object>());
      throw;
    }
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    await base.StopAsync(cancellationToken);

    await _discordSocketClient.StopAsync().ConfigureAwait(false);

    Log.Logger.Fatal("Discord client has been stopped.");
  }

  private void InitializeHowbotServices(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      _logger.LogDebug("Starting initialization of Howbot services..");

      _serviceProvider.GetRequiredService<IDiscordClientService>()?.Initialize();
      _serviceProvider.GetRequiredService<ILavaNodeService>()?.Initialize();
      _serviceProvider.GetRequiredService<IInteractionHandlerService>()?.Initialize();
      _serviceProvider.GetRequiredService<IEmbedService>()?.Initialize();
      _serviceProvider.GetRequiredService<IMusicService>()?.Initialize();

      using var scope = _serviceProvider.CreateScope();
      scope.ServiceProvider.GetRequiredService<IDatabaseService>()?.Initialize();
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, nameof(InitializeHowbotServices));
      throw;
    }
  }
}
