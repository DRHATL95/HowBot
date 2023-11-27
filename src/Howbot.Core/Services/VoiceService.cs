using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Options;

namespace Howbot.Core.Services;

public class VoiceService : ServiceBase<VoiceService>, IVoiceService
{
  private readonly IAudioService _audioService;

  public VoiceService(IAudioService audioService, ILoggerAdapter<VoiceService> logger) : base(logger)
  {
    _audioService = audioService;
  }

  public async ValueTask<CommandResponse> JoinVoiceChannelAsync(IGuildUser user, IGuildChannel channel)
  {
    try
    {
      var voiceChannel = user.VoiceChannel;
      var guildTag = DiscordHelper.GetGuildTag(user.Guild);

      Logger.LogDebug("Attempting to join voice channel {0}.", guildTag);

      _ = await GetPlayerAsync(new GetPlayerParameters
      {
        ConnectToVoiceChannel = true,
        GuildId = channel.GuildId,
        TextChannel = (ITextChannel)channel,
        VoiceChannelId = voiceChannel?.Id ?? 0
      }).ConfigureAwait(false);

      Logger.LogDebug("Successfully joined voice channel {0}.", guildTag);

      return CommandResponse.CommandSuccessful("Successfully joined voice channel.", true);
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(JoinVoiceChannelAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async ValueTask<CommandResponse> LeaveVoiceChannelAsync(IGuildUser user, IGuildChannel guildChannel)
  {
    try
    {
      var player = await GetPlayerAsync(new GetPlayerParameters()
      {
        ConnectToVoiceChannel = false,
        GuildId = guildChannel.GuildId,
        TextChannel = (ITextChannel)guildChannel,
        VoiceChannelId = user.VoiceChannel.Id
      }).ConfigureAwait(false);

      if (player is null)
      {
        return CommandResponse.CommandNotSuccessful("Unable to leave channel. Not in a voice channel.");
      }

      // Using lavalink player disconnect from the voice channel.
      await player.DisconnectAsync().ConfigureAwait(false);

      // Return successful response
      return CommandResponse.CommandSuccessful("Successfully disconnected from voice channel.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(LeaveVoiceChannelAsync));
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  private async ValueTask<ILavalinkPlayer> GetPlayerAsync(GetPlayerParameters playerParams)
  {
    ArgumentNullException.ThrowIfNull(playerParams);

    if (playerParams.VoiceChannelId == 0)
    {
      throw new ArgumentException("Voice channel id cannot be 0.", nameof(playerParams));
    }

    var retrieveOptions = new PlayerRetrieveOptions(
      ChannelBehavior: playerParams.ConnectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

    var result = await _audioService.Players
      .RetrieveAsync(guildId: playerParams.GuildId, memberVoiceChannel: playerParams.VoiceChannelId,
        playerFactory: PlayerFactory.Default,
        options: new OptionsWrapper<LavalinkPlayerOptions>(new LavalinkPlayerOptions()),
        retrieveOptions: retrieveOptions);

    if (result.IsSuccess)
    {
      return result.Player;
    }

    var errorMessage = result.Status switch
    {
      PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
      PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
      _ => "Unknown error.",
    };

    await playerParams.TextChannel.SendMessageAsync(errorMessage).ConfigureAwait(false);

    return null;
  }

  private readonly struct GetPlayerParameters
  {
    public ulong GuildId { get; init; }

    public ulong VoiceChannelId { get; init; }

    public ITextChannel TextChannel { get; init; }

    public bool ConnectToVoiceChannel { get; init; }
  }
}
