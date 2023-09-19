using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Microsoft.Extensions.Options;
using static System.Threading.SpinWait;

namespace Howbot.Core.Services;

public class VoiceService : ServiceBase<VoiceService>, IVoiceService
{
  // Instance variables
  [NotNull] private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;

  [NotNull] private readonly ILoggerAdapter<VoiceService> _logger;

  [NotNull] private readonly IAudioService _audioService;

  public VoiceService([NotNull] ILoggerAdapter<VoiceService> logger, [NotNull] IAudioService audioService) : base(logger)
  {
    _logger = logger;
    _audioService = audioService;
    _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
  }

  public async Task<CommandResponse> JoinVoiceChannelAsync(IGuildUser user, bool isDeaf = true)
  {
    try
    {
      var voiceChannel = user.VoiceChannel;
      if (voiceChannel is null)
      {
        return CommandResponse.CommandNotSuccessful("User who requested command is not in voice.");
      }

      await _audioService.Players.JoinAsync(voiceChannel: voiceChannel).ConfigureAwait(false);

      return CommandResponse.CommandSuccessful("This command is not active.");
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Exception has been thrown executing command [{CommandName}]",
        nameof(JoinVoiceChannelAsync));
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
      _logger.LogError(exception, "Exception thrown in VoiceService.LeaveVoiceChannelAsync");
      return CommandResponse.CommandNotSuccessful(exception);
    }
  }

  public async Task InitiateDisconnectLogicAsync(ILavalinkPlayer player, TimeSpan timeSpan)
  {
    if (!_disconnectTokens.TryGetValue(player.VoiceChannelId, out var value))
    {
      value = new CancellationTokenSource();
      _disconnectTokens.TryAdd(player.VoiceChannelId, value);
    }
    else if (value.IsCancellationRequested)
    {
      _disconnectTokens.TryUpdate(player.VoiceChannelId, new CancellationTokenSource(), value);
      value = _disconnectTokens[player.VoiceChannelId];
    }

    var isCancelled = SpinUntil(() => value?.IsCancellationRequested ?? false, timeSpan);
    if (isCancelled)
    {
      _logger.LogDebug("Auto disconnect cancelled.");
      return;
    }

    await player.DisconnectAsync(value.Token).ConfigureAwait(false);
  }

  private struct GetPlayerParameters
  {
    public ulong GuildId { get; init; }

    public ulong VoiceChannelId { get; init; }

    [NotNull] public ITextChannel TextChannel { get; init; }

    public bool ConnectToVoiceChannel { get; init; }
  }

  private async ValueTask<ILavalinkPlayer> GetPlayerAsync(GetPlayerParameters playerParams)
  {
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
}
