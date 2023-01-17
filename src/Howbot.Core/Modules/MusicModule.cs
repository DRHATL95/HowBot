using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Responses.Search;

namespace Howbot.Core.Modules;

public class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly ILoggerAdapter<MusicModule> _logger;
  private readonly IServiceLocator _serviceLocator;

  public MusicModule(ILoggerAdapter<MusicModule> logger, IServiceLocator serviceLocator)
  {
    _logger = logger;
    _serviceLocator = serviceLocator;
  }

  [SlashCommand(Constants.Commands.JoinCommandName, Constants.Commands.JoinCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoiceCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoiceCommandPermission)]
  public async Task JoinCommandAsync()
  {
    try
    {
      await this.DeferAsync(true);

      using var scope = _serviceLocator.CreateScope();
      var voiceService = scope.ServiceProvider.GetRequiredService<IVoiceService>();

      var result = await voiceService.JoinVoiceAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);

      if (!result.Success)
      {
        if (result.Exception != null) throw result.Exception;

        if (!string.IsNullOrEmpty(result.Message)) _logger.LogDebug(result.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = "Command did not run successfully.");
      }
      else
      {
        await DeleteOriginalResponseAsync();
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown executing command [{CommandName}]", nameof(JoinCommandAsync));
      throw;
    }
  }

  [SlashCommand(Constants.Commands.PlayCommandName, Constants.Commands.PlayCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoicePlayCommandPermission)]
  public async Task PlayCommandAsync(
    [Summary(Constants.Commands.PlaySearchRequestArgumentName, Constants.Commands.PlaySearchRequestArgumentDescription)] string searchRequest,
    [Summary(Constants.Commands.PlaySearchTypeArgumentName, Constants.Commands.PlaySearchTypeArgumentDescription)] SearchType? searchType = null)
  {
    try
    {
      await DeferAsync(true);

      if (string.IsNullOrEmpty(searchRequest))
      {
        await RespondAsync("You must enter a search request!", ephemeral: true);
        return;
      }

      using var scope = _serviceLocator.CreateScope();
      var musicService = scope.ServiceProvider.GetRequiredService<IMusicService>();

      CommandResponse commandResponse;
      var user = Context.User as IGuildUser;
      var voiceState = Context.User as IVoiceState;
      var channel = Context.Channel as ITextChannel;

      if (!searchType.HasValue)
      {
        // Default to YouTube
        commandResponse = await musicService.PlayBySearchTypeAsync(SearchType.YouTube, searchRequest, user, voiceState, channel);
      }
      else
      {
        commandResponse = await musicService.PlayBySearchTypeAsync(searchType.Value, searchRequest, user, voiceState, channel);
      }

      if (!commandResponse.Success)
      {
        if (commandResponse.Exception != null) throw commandResponse.Exception;

        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogInformation(commandResponse.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        _logger.LogDebug("Else");
        // Should create embed somewhere after this point
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown playing song");
      throw;
    }
  }

  [SlashCommand(Constants.Commands.PauseCommandName, Constants.Commands.PauseCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoicePlayCommandPermission)]
  public async Task PauseCommandAsync()
  {
    try
    {
      await DeferAsync(ephemeral: true);

      using var scope = _serviceLocator.CreateScope();
      var musicService = scope.ServiceProvider.GetRequiredService<IMusicService>();

      var commandResponse = await musicService.PauseCurrentPlayingTrackAsync(Context.Guild);
    }
    catch (Exception exception)
    {
      Console.WriteLine(exception);
      throw;
    }
  }

  [SlashCommand(Constants.Commands.ResumeCommandName, Constants.Commands.ResumeCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoicePlayCommandPermission)]
  public async Task ResumeCommandAsync()
  {
    try
    {
      await DeferAsync(ephemeral: true);
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }
}
