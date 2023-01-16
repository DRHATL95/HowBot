using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Victoria.Node;
using Victoria.Player;

namespace Howbot.Core.Services;

public class MusicService : IMusicService, IVoiceService
{
  private readonly ILoggerAdapter<MusicService> _logger;
  private readonly LavaNode _lavaNode;
  private readonly IServiceLocator _serviceLocator;

  public MusicService(ILoggerAdapter<MusicService> logger, LavaNode lavaNode, IServiceLocator serviceLocator)
  {
    _logger = logger;
    _lavaNode = lavaNode;
    _serviceLocator = serviceLocator;
  }
  
  public async Task<CommandResponse> JoinVoiceCommandAsync(IGuildUser user, ITextChannel textChannel)
  {
    if (user.Guild == null) throw new NullReferenceException(nameof(user.Guild));

    try
    {
      if (user is IVoiceState voiceState)
      {
        var player = await this.JoinGuildVoiceChannel(voiceState, textChannel);
        if (player != null)
        {
          return CommandResponse.CommandSuccessful();
        }
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception has been thrown executing command [{CommandName}]", nameof(JoinVoiceCommandAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }

    return CommandResponse.CommandNotSuccessful();
  }

  [ItemCanBeNull]
  private async Task<LavaPlayer> JoinGuildVoiceChannel(IVoiceState voiceState, ITextChannel textChannel)
  {
    if (voiceState == null) throw new ArgumentNullException(nameof(voiceState));
    if (textChannel == null) throw new ArgumentNullException(nameof(textChannel));
    
    // Check if user is in a voice channel
    if (voiceState.VoiceChannel == null)
    {
      _logger.LogInformation("Requested user is not in a voice channel");
      return null;
    }

    using var scope = _serviceLocator.CreateScope();
    var lavaNode = scope.ServiceProvider.GetRequiredService<LavaNode>();

    return (await lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel) as LavaPlayer);
  }
}
