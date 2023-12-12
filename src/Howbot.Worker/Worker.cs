using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Exceptions;
using Howbot.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Howbot.Worker;

public class Worker(
  IDiscordClientService discordClientService,
  IServiceProvider serviceProvider,
  DiscordSocketClient discordSocketClient,
  ILoggerAdapter<Worker> logger)
  : BackgroundService
{
  /// <summary>
  ///   Executes the worker service asynchronously.
  /// </summary>
  /// <param name="cancellationToken">The cancellation token to stop the execution of the worker service.</param>
  /// <exception cref="DiscordLoginException">Thrown if there is an error while logging into Discord.</exception>
  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      InitializeHowbotServices(cancellationToken);

      if (!await discordClientService.LoginDiscordBotAsync(Configuration.DiscordToken).ConfigureAwait(false))
      {
        logger.LogError("Unable to login to discord API with token.");

        // Stop worker, cannot continue without being authenticated
        await StopAsync(cancellationToken).ConfigureAwait(false);

        throw new DiscordLoginException("Unable to login to discord API with token.");
      }

      await discordClientService.StartDiscordBotAsync().ConfigureAwait(false);

      // Run worker service indefinitely until cancellationToken is created or process is manually stopped.
      await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
    catch (DiscordLoginException loginException)
    {
      logger.LogError(loginException, "An exception has been thrown logging into Discord.");
      throw;
    }
    catch (Exception exception)
    {
      logger.LogError(exception, "An exception has been thrown in the main worker");
      throw;
    }
  }

  /// <summary>
  ///   Stops the asynchronous operation by stopping the Discord client and logs the event.
  /// </summary>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    await base.StopAsync(cancellationToken);

    await discordSocketClient.StopAsync().ConfigureAwait(false);

    logger.LogCritical("Discord client has been stopped");
  }

  /// <summary>
  ///   Initializes the Howbot services.
  /// </summary>
  /// <param name="cancellationToken">The cancellation token.</param>
  private void InitializeHowbotServices(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      serviceProvider.GetRequiredService<IDiscordClientService>()?.Initialize();
      serviceProvider.GetRequiredService<ILavaNodeService>()?.Initialize();
      serviceProvider.GetRequiredService<IInteractionHandlerService>()?.Initialize();
      serviceProvider.GetRequiredService<IEmbedService>()?.Initialize();
      serviceProvider.GetRequiredService<IMusicService>()?.Initialize();

      using var scope = serviceProvider.CreateScope();
      scope.ServiceProvider.GetRequiredService<IDatabaseService>()?.Initialize();
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(InitializeHowbotServices));
      throw;
    }
  }
}
