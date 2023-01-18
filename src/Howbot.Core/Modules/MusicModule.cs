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

      var commandResponse = await voiceService.JoinVoiceAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);
      
      if (!commandResponse.Success)
      {
        if (commandResponse.Exception != null) throw commandResponse.Exception;

        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

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
      // var embedService = scope.ServiceProvider.GetRequiredService<IEmbedService>();

      var commandResponse = await musicService.PauseTrackAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(PauseCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = Messages.Responses.BotTrackPaused);
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Error has been thrown");
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

      using var scope = _serviceLocator.CreateScope();
      var musicService = scope.ServiceProvider.GetRequiredService<IMusicService>();

      var commandResponse = await musicService.ResumeTrackAsync(Context.Guild);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(ResumeCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        // TODO: Embed or ReplyAsync
        await ModifyOriginalResponseAsync(properties => properties.Content = "Resuming track");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand("seek", "Seek something", true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  [RequireBotPermission(Permissions.Bot.GuildBotVoicePlayCommandPermission)]
  [RequireUserPermission(Permissions.User.GuildUserVoicePlayCommandPermission)]
  public async Task SeekCommandAsync(TimeSpan timeToSeek)
  {
    try
    {
      await DeferAsync(ephemeral: true);

      using var scope = _serviceLocator.CreateScope();
      var musicService = scope.ServiceProvider.GetRequiredService<IMusicService>();

      var commandResponse = await musicService.SeekTrackAsync(Context.Guild, timeToSeek);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(SeekCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        // TODO: Embed or RespondAsync
        await ModifyOriginalResponseAsync(properties => properties.Content = "Seeking to desired location");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand("skip", "skip either to next track or a valid number of tracks in the queue", true, RunMode.Async)]
  public async Task SkipCommandAsync(int? tracksToSkip = null)
  {
    try
    {
      await DeferAsync(ephemeral: true);

      using var scope = _serviceLocator.CreateScope();
      var musicService = scope.ServiceProvider.GetRequiredService<IMusicService>();

      var commandResponse = await musicService.SkipTrackAsync(Context.Guild, tracksToSkip ?? 0);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(SkipCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "Skipping to spot in queue");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand("volume", "Change volume", true, RunMode.Async)]
  public async Task VolumeCommandAsync(int? newVolume = 0)
  {
    try
    {
      await DeferAsync(ephemeral: true);

      using var scope = _serviceLocator.CreateScope();
      var musicService = scope.ServiceProvider.GetRequiredService<IMusicService>();

      var commandResponse = await musicService.ChangeVolumeAsync(Context.Guild, newVolume ?? 0);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(VolumeCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "Changing volume");
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception);
      throw;
    }
  }

  [SlashCommand("np", "Gets an embed of the current playing track", true, RunMode.Async)]
  public async Task NowPlayingCommandAsync()
  {
    try
    {
      await DeferAsync(ephemeral: true);

      using var scope = _serviceLocator.CreateScope();
      var musicService = scope.ServiceProvider.GetRequiredService<IMusicService>();

      var commandResponse = await musicService.NowPlayingAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);

      if (!commandResponse.Success)
      {
        _logger.LogCommandFailed(nameof(NowPlayingCommandAsync));

        if (commandResponse.Exception != null) throw commandResponse.Exception;
        
        if (!string.IsNullOrEmpty(commandResponse.Message)) _logger.LogDebug(commandResponse.Message);

        await ModifyOriginalResponseAsync(properties => properties.Content = commandResponse.Message);
      }
      else
      {
        if (commandResponse.Embed != null)
        {
          await ModifyOriginalResponseAsync(properties => properties.Embed = commandResponse.Embed as Embed);
        }
        // await ModifyOriginalResponseAsync(properties => properties.Content = "Skipping to spot in queue");
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }
}
