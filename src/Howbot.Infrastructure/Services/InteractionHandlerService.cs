using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Microsoft.Extensions.Logging;

namespace Howbot.Infrastructure.Services;

public class InteractionHandlerService(
  IServiceProvider services,
  InteractionService interactionService,
  ILoggerAdapter<InteractionHandlerService> logger)
  : ServiceBase<InteractionHandlerService>(logger), IInteractionHandlerService, IDisposable
{
  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    await AddModulesToDiscordBotAsync();

    interactionService.Log += InteractionServiceOnLog;
    interactionService.InteractionExecuted += InteractionServiceOnInteractionExecuted;

    // _interactionService.SlashCommandExecuted += InteractionServiceOnSlashCommandExecuted;
    // _interactionService.ContextCommandExecuted += InteractionServiceOnContextCommandExecuted;
    // _interactionService.AutocompleteCommandExecuted += InteractionServiceOnAutocompleteCommandExecuted;
    // _interactionService.AutocompleteHandlerExecuted += InteractionServiceOnAutocompleteHandlerExecuted;
  }

  private async Task AddModulesToDiscordBotAsync()
  {
    try
    {
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();
      var assembly = assemblies.FirstOrDefault(x => !string.IsNullOrEmpty(x.FullName) && x.FullName.Contains("Howbot.Core"));

      var modules = await interactionService.AddModulesAsync(assembly, services);
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

  private Task InteractionServiceOnLog(LogMessage logMessage)
  {
    try
    {
      var logLevel = DiscordHelper.ConvertLogSeverityToLogLevel(logMessage.Severity);
      if (logLevel is LogLevel.Error)
      {
        throw logMessage.Exception ?? new Exception(logMessage.Message);
      }

      Logger.Log(logLevel, logMessage.Message);

      return Task.CompletedTask;
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(InteractionServiceOnLog));

      return Task.FromException(exception);
    }
  }

  private async Task InteractionServiceOnInteractionExecuted(ICommandInfo commandInfo,
    IInteractionContext interactionContext, IResult result)
  {
    try
    {
      if (!result.IsSuccess)
      {
        if (interactionContext.Interaction is SocketInteraction socketInteraction)
        {
          await DiscordHelper.HandleSocketInteractionErrorAsync(socketInteraction, result,
            Logger);
        }
      }
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(InteractionServiceOnInteractionExecuted));
    }
  }

  public void Dispose()
  {
    interactionService.Log -= InteractionServiceOnLog;
    interactionService.InteractionExecuted -= InteractionServiceOnInteractionExecuted;

    GC.SuppressFinalize(this);
  }
}
