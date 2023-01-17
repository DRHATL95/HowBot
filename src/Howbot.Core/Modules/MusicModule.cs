using System;
using System.Runtime.InteropServices;
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

  [SlashCommand("join", "Join a voice channel within a Guild.", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.Connect | GuildPermission.ViewChannel)]
  [RequireUserPermission(GuildPermission.Connect | GuildPermission.ViewChannel)]
  public async Task JoinAsync()
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
      _logger.LogError(exception, "Exception thrown executing command [{CommandName}]", nameof(JoinAsync));
      throw;
    }
  }

  [SlashCommand("play", "Play a search request using the given search providers. Defaults to YouTube.", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(GuildPermission.Connect | GuildPermission.ViewChannel | GuildPermission.Speak)]
  [RequireUserPermission(GuildPermission.Connect | GuildPermission.ViewChannel | GuildPermission.Speak)]
  public async Task PlayAsync(
    [Summary("search_request", "Search request used to search through audio providers.")] string searchRequest,
    [Summary("search_type", "Audio providers used with search request. Default is YouTube.")] SearchType? searchType = null)
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
        await DeleteOriginalResponseAsync();
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown playing song");
      throw;
    }
  }
}
