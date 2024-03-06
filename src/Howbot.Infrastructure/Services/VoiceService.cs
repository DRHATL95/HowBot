using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Helpers;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Commands;
using Howbot.Core.Services;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Options;

namespace Howbot.Infrastructure.Services;

public class VoiceService(IAudioService audioService, ILoggerAdapter<VoiceService> logger)
  : ServiceBase<VoiceService>(logger), IVoiceService
{
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
      });

      Logger.LogDebug("Successfully joined voice channel {0}.", guildTag);

      return CommandResponse.Create(true, "Successfully joined voice channel.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(JoinVoiceChannelAsync));
      return CommandResponse.Create(false, exception: exception);
    }
  }

  public async ValueTask<CommandResponse> LeaveVoiceChannelAsync(IGuildUser user, IGuildChannel guildChannel)
  {
    try
    {
      var player = await GetPlayerAsync(new GetPlayerParameters
      {
        ConnectToVoiceChannel = false,
        GuildId = guildChannel.GuildId,
        TextChannel = (ITextChannel)guildChannel,
        VoiceChannelId = user.VoiceChannel.Id
      });

      if (player is null)
      {
        return CommandResponse.Create(false, "Unable to leave channel. Not in a voice channel.");
      }

      await player.DisconnectAsync();

      return CommandResponse.Create(true, "Successfully disconnected from voice channel.");
    }
    catch (Exception exception)
    {
      Logger.LogError(exception, nameof(LeaveVoiceChannelAsync));
      return CommandResponse.Create(false, exception: exception);
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
      playerParams.ConnectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

    var result = await audioService.Players
      .RetrieveAsync(playerParams.GuildId, playerParams.VoiceChannelId,
        PlayerFactory.Default,
        new OptionsWrapper<LavalinkPlayerOptions>(new LavalinkPlayerOptions()),
        retrieveOptions);

    if (result.IsSuccess)
    {
      return result.Player;
    }

    var errorMessage = result.Status switch
    {
      PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
      PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
      _ => "Unknown error."
    };

    await playerParams.TextChannel.SendMessageAsync(errorMessage);

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
