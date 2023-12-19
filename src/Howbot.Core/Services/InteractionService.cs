using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Howbot.Core.Interfaces;

namespace Howbot.Core.Services;

public class InteractionService : Discord.Interactions.InteractionService, IInteractionService
{
  private readonly IServiceProvider _services;

  public InteractionService(DiscordSocketClient discord, IServiceProvider services,
    ILoggerAdapter<InteractionService> logger, InteractionServiceConfig config = null) : base(discord, config)
  {
    _services = services;
    Logger = logger;
  }

  public InteractionService(DiscordShardedClient discord, IServiceProvider services,
    ILoggerAdapter<InteractionService> logger, InteractionServiceConfig config = null) : base(discord, config)
  {
    _services = services;
    Logger = logger;
  }

  public InteractionService(BaseSocketClient discord, IServiceProvider services,
    ILoggerAdapter<InteractionService> logger, InteractionServiceConfig config = null) : base(discord, config)
  {
    _services = services;
    Logger = logger;
  }

  public InteractionService(DiscordRestClient discord, IServiceProvider services,
    ILoggerAdapter<InteractionService> logger, InteractionServiceConfig config = null) : base(discord, config)
  {
    Logger = logger;
    _services = services;
  }

  public ILoggerAdapter<InteractionService> Logger { get; }

  public async void Initialize()
  {
    await AddModulesToDiscordBotAsync();
  }

  public Task RegisterCommandsToGuildAsync(ulong discordDevelopmentGuildId)
  {
    return base.RegisterCommandsToGuildAsync(discordDevelopmentGuildId);
  }

  public Task RegisterCommandsGloballyAsync()
  {
    return base.RegisterCommandsGloballyAsync();
  }

  public Task<IResult> ExecuteCommandAsync(SocketInteractionContext context)
  {
    return base.ExecuteCommandAsync(context, _services);
  }

  private async Task AddModulesToDiscordBotAsync()
  {
    try
    {
      var modules = await AddModulesAsync(Assembly.GetExecutingAssembly(), _services).ConfigureAwait(false);
      if (!modules.Any())
      {
        throw new Exception("No modules were added to the Discord bot.");
      }
    }
    catch (FileNotFoundException exception)
    {
      Logger.LogError(exception, "Unable to find the assembly. Value: {AssemblyName}",
        Assembly.GetEntryAssembly()?.ToString());
      throw;
    }
    catch (Exception e)
    {
      Logger.LogError(e, nameof(AddModulesToDiscordBotAsync));
      throw;
    }
  }
}
