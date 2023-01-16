using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Interfaces;
using Howbot.Core.Services;
using Microsoft.Extensions.DependencyInjection;

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
      var musicService = scope.ServiceProvider.GetRequiredService<IMusicService>();

      var result = await musicService.JoinVoiceCommandAsync(Context.User as IGuildUser, Context.Channel as ITextChannel);

      if (!result.Success)
      {
        _logger.LogError("Module Slash Command [{CommandName}] has failed", nameof(JoinAsync));

        if (result.Exception != null)
        {
          // TODO: dhoward - what to do w/ exceptions up to here
          throw result.Exception;
        }

        if (!string.IsNullOrEmpty(result.Message))
        {
          _logger.LogDebug(result.Message);
        }

        await ModifyOriginalResponseAsync(properties => properties.Content = "Command did not run successfully.");
      }
      else
      {
        _logger.LogDebug("Command ran successfully.");
        await DeleteOriginalResponseAsync();
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception thrown executing command [{CommandName}]", nameof(JoinAsync));
      throw;
    }
  }
}
