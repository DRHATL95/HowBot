using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Exceptions;
using Howbot.Core.Services;
using Howbot.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Infrastructure.Services;

public class HowbotService(DiscordSocketClient discordSocketClient, IDiscordClientService discordClientService, IServiceProvider serviceProvider, ILoggerAdapter<HowbotService> logger) : ServiceBase<HowbotService>(logger), IHowbotService, IDisposable, IAsyncDisposable
{
  // TODO: This will be used to store the session IDs of the guilds that the bot is connected to
  // IMPORTANT: The music functionality requires session IDs to get the player for the guild
  public ConcurrentDictionary<ulong, string> SessionIds { get; set; } = new();
  
  public new void Initialize()
  {
    base.Initialize();
  }

  public async Task StartWorkerServiceAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      
      InitializeHowbotServices(cancellationToken);

      await LoginBotToDiscordAsync(Configuration.DiscordToken, cancellationToken);

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

      await discordSocketClient.StopAsync();
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
      await discordClientService.StartDiscordBotAsync();
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(StartDiscordBotAsync));
      throw;
    }
  }
  
  private void InitializeHowbotServices(CancellationToken cancellationToken = default)
  {
    try
    {
      cancellationToken.ThrowIfCancellationRequested();

      serviceProvider.GetRequiredService<IHowbotService>()?.Initialize();
      serviceProvider.GetRequiredService<IDiscordClientService>()?.Initialize();
      serviceProvider.GetRequiredService<ILavaNodeService>()?.Initialize();
      serviceProvider.GetRequiredService<IInteractionHandlerService>()?.Initialize();
      serviceProvider.GetRequiredService<IEmbedService>()?.Initialize();
      serviceProvider.GetRequiredService<IMusicService>()?.Initialize();
      serviceProvider.GetRequiredService<IInteractionService>()?.Initialize();

      using var scope = serviceProvider.CreateScope();
      scope.ServiceProvider.GetRequiredService<IDatabaseService>()?.Initialize();
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(InitializeHowbotServices));
      throw;
    }
  }

  private async Task ProcessMessageAsync(string message)
  {
    try
    {
      await Task.CompletedTask;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(ProcessMessageAsync));
      throw;
    }
  }
  
  public void Dispose()
  {
    discordSocketClient?.Dispose();
  }

  public async ValueTask DisposeAsync()
  {
    if (discordSocketClient != null)
    {
      await discordSocketClient.DisposeAsync();
    }
  }
}
