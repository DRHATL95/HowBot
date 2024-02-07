﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;

namespace Howbot.Core.Services;

public class InteractionService : Discord.Interactions.InteractionService, IInteractionService
{
  private readonly IServiceProvider _services;

  private ILoggerAdapter<InteractionService> Logger { get; }

  public async Task Initialize()
  {
    await AddModulesToDiscordBotAsync();

    InteractionExecuted += OnInteractionExecuted;
  }

  public async Task RegisterCommandsToGuildAsync(ulong discordDevelopmentGuildId)
  {
    await base.RegisterCommandsToGuildAsync(discordDevelopmentGuildId);
  }

  public async Task RegisterCommandsGloballyAsync()
  {
    await base.RegisterCommandsGloballyAsync();
  }

  public async Task<IResult> ExecuteCommandAsync(SocketInteractionContext context)
  {
    return await base.ExecuteCommandAsync(context, _services);
  }

  private async Task OnInteractionExecuted(ICommandInfo commandInfo, IInteractionContext interactionContext,
    IResult result)
  {
    if (!result.IsSuccess)
    {
      await DiscordHelper.HandleSocketInteractionErrorAsync(interactionContext.Interaction as SocketInteraction, result,
        Logger);
    }
  }

  private async Task AddModulesToDiscordBotAsync()
  {
    try
    {
      var modules = await AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
      if (!modules.Any())
      {
        throw new Exception("No modules were added to the Discord bot.");
      }
    }
    catch (Exception e)
    {
      Logger.LogError(e, nameof(AddModulesToDiscordBotAsync));
      throw;
    }
  }

  #region Constructors

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

  #endregion
}
