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
  [NotNull] private readonly IServiceProvider _serviceProvider;
  [NotNull] private readonly DiscordSocketClient _discordSocketClient;

  public Worker([NotNull] IDiscordClientService discordClientService, [NotNull] IServiceProvider serviceProvider, DiscordSocketClient discordSocketClient)
  {
    _discordClientService = discordClientService;
    _serviceProvider = serviceProvider;
    _discordSocketClient = discordSocketClient;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    try
    {
      InitializeHowbotServices(cancellationToken);

      if (!await _discordClientService.LoginDiscordBotAsync(Configuration.DiscordToken).ConfigureAwait(false))
      {
        Log.Fatal(
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
      Log.Error(loginException, "An exception has been thrown logging into Discord.");
      throw;
    }
    catch (Exception exception)
    {
      Log.Error(exception, "An exception has been thrown in the main worker");
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

  private void InitializeHowbotServices(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    Log.Debug("Starting initialization of Howbot services");

    try
    {
      // Call each service 'initialize' function primarily used for hooking up events
      using var scope = _serviceProvider.CreateScope();

      scope.ServiceProvider.GetRequiredService<IDiscordClientService>()?.Initialize();
      scope.ServiceProvider.GetRequiredService<IInteractionHandlerService>()?.Initialize();
    }
    catch (Exception exception)
    {
      Log.Error(exception, nameof(InitializeHowbotServices));
      throw;
    }
  }
  
}
