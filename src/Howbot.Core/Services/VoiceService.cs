using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Entities;
using Howbot.Core.Interfaces;
using Victoria.Node;
using Victoria.Player;

namespace Howbot.Core.Services;

public class VoiceService : IVoiceService
{
  private readonly LavaNode<Player<LavaTrack>, LavaTrack> _lavaNode;
  private readonly ILoggerAdapter<VoiceService> _logger;

  public VoiceService(LavaNode<Player<LavaTrack>, LavaTrack> lavaNode, ILoggerAdapter<VoiceService> logger)
  {
    _lavaNode = lavaNode;
    _logger = logger;
  }
  
  public void Initialize()
  {
    // TODO:
  }
  
  public async Task<CommandResponse> JoinVoiceAsync(IGuildUser user, ITextChannel textChannel)
  {
    try
    {
      if (user is IVoiceState voiceState)
      {
        var lavaPlayer = await this.JoinGuildVoiceChannelAsync(voiceState, textChannel);

        return lavaPlayer == null ? CommandResponse.CommandNotSuccessful(new Exception("Exception thrown creating lava player while joining voice channel")) : CommandResponse.CommandSuccessful(lavaPlayer);
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception has been thrown executing command [{CommandName}]", nameof(JoinVoiceAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }

    return CommandResponse.CommandNotSuccessful("Unable to join voice channel.");
  }

  public async Task<CommandResponse> LeaveVoiceChannelAsync(IGuildUser user)
  {
    try
    {
      if (user is not IVoiceState voiceState)
      {
        return CommandResponse.CommandNotSuccessful("Unable to leave voice channel");
      }

      // Get lava player instance
      if(!_lavaNode.HasPlayer(user.Guild))
      {
        _logger.LogDebug("Cannot leave voice, not in voice channel.");
        return CommandResponse.CommandNotSuccessful(Messages.Responses.BotNotConnectedToVoiceResponseMessage);
      }
        
      _logger.LogDebug("Leaving voice channel");
      await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
      return CommandResponse.CommandSuccessful();

    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception has been thrown executing command [{CommandName}]", nameof(LeaveVoiceChannelAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }
  
  private async Task<Player<LavaTrack>> JoinGuildVoiceChannelAsync(IVoiceState voiceState, ITextChannel textChannel)
  {
    // Parameter error handling
    if (voiceState == null) throw new ArgumentNullException(nameof(voiceState));
    if (textChannel == null) throw new ArgumentNullException(nameof(textChannel));

    // Check if user is in a voice channel
    if (voiceState.VoiceChannel == null)
    {
      _logger.LogInformation("Requested user is not in a voice channel");
      return null;
    }
    
    // Check if bot is already connect
    if (!IsBotAlreadyConnected(textChannel.Guild))
    {
      // Not in voice, join active voice channel
      return await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
    }

    // Get already created lava player (already connected to voice channel)
    _lavaNode.TryGetPlayer(textChannel.Guild, out var player);
    return player;
  }
  
  private bool IsBotAlreadyConnected(IGuild guild)
  {
    if (guild == null) throw new ArgumentNullException(nameof(guild));
    
    return _lavaNode.HasPlayer(guild);
  }
  
}
