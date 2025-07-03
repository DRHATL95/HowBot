using Discord.WebSocket;
using Howbot.Application.Interfaces.Discord;
using Howbot.Application.Interfaces.Infrastructure;
using Howbot.Application.Interfaces.Lavalink;
using Howbot.Infrastructure.Discord.Exceptions;
using Howbot.Infrastructure.Services.Abstract;
using Howbot.SharedKernel;
using Howbot.SharedKernel.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Howbot.Infrastructure.Services.Concrete;

public class HowbotService(
  IOptions<BotSettings> botSettings,
  DiscordSocketClient discordClient,
  IDiscordClientService discordClientService,
  IServiceProvider serviceProvider,
  ILoggerAdapter<HowbotService> logger) : ServiceBase<HowbotService>(logger), IHowbotService, IDisposable
{
  public void Dispose()
  {
    discordClient?.Dispose();

    GC.SuppressFinalize(this);
  }

  public async Task StartWorkerServiceAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      await InitializeHowbotServicesAsync();

      await LoginBotToDiscordAsync(botSettings.Value.DiscordToken, cancellationToken);

      await StartDiscordBotAsync(cancellationToken);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(StartWorkerServiceAsync));
      throw;
    }
  }

  public async Task StopWorkerServiceAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      await discordClient.StopAsync();
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(StopWorkerServiceAsync));
      throw;
    }
  }

  private async Task LoginBotToDiscordAsync(string discordToken, CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      await discordClientService.LoginDiscordBotAsync(discordToken);
    }
    catch (DiscordLoginException discordLoginException)
    {
      Logger.LogError(discordLoginException, nameof(LoginBotToDiscordAsync));
      throw;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(LoginBotToDiscordAsync));
      throw;
    }
  }

  private async Task StartDiscordBotAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      await discordClientService.StartDiscordBotAsync();
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(StartDiscordBotAsync));
      throw;
    }
  }

  private async Task InitializeHowbotServicesAsync()
  {
    try
    {
      await Task.Run(async () =>
      {
        serviceProvider.GetRequiredService<IHowbotService>()?.Initialize();
        serviceProvider.GetRequiredService<IDiscordClientService>()?.Initialize();
        serviceProvider.GetRequiredService<ILavaNodeService>()?.Initialize();
        serviceProvider.GetRequiredService<IEmbedService>()?.Initialize();
        serviceProvider.GetRequiredService<IMusicService>()?.Initialize();

        var interactionHandlerService = serviceProvider.GetRequiredService<IInteractionHandlerService>();
        if (interactionHandlerService != null)
        {
          await interactionHandlerService.InitializeAsync();
        }

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IDatabaseService>()?.Initialize();
      });
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(InitializeHowbotServicesAsync));
      throw;
    }
  }
}
