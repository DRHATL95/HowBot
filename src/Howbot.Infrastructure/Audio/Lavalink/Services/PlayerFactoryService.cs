using System.Collections.Immutable;
using Ardalis.GuardClauses;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Application.Interfaces.Lavalink;
using Howbot.Application.Models.Lavalink.Players;
using Howbot.SharedKernel;
using Lavalink4NET;
using Lavalink4NET.Clients;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Microsoft.Extensions.Options;

namespace Howbot.Infrastructure.Audio.Lavalink.Services;

public class PlayerFactoryService(IAudioService audioService, ILoggerAdapter<PlayerFactoryService> logger) : IPlayerFactoryService
{
  public async ValueTask<HowbotPlayer?> GetOrCreatePlayerAsync(ulong guildId, ulong? voiceChannelId, bool allowConnect = false, bool requireChannel = true, ImmutableArray<IPlayerPrecondition> preconditions = default, CancellationToken token = default)
  {
    token.ThrowIfCancellationRequested();
    
    try
    {
      var playerOptions = new HowbotPlayerOptions
      {
        DisconnectOnDestroy = true,
        DisconnectOnStop = false,
        SelfDeaf = true,
        ClearQueueOnStop = false,
        ClearHistoryOnStop = false,
        InitialVolume = 100.0f,
        IsAutoPlayEnabled = false
      };

      var retrieveOptions = new PlayerRetrieveOptions(
        allowConnect ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None,
        requireChannel ? MemberVoiceStateBehavior.RequireSame : MemberVoiceStateBehavior.Ignore,
        preconditions);

      var playerResult = await audioService.Players
        .RetrieveAsync<HowbotPlayer, HowbotPlayerOptions>(guildId, voiceChannelId, CreatePlayerAsync, new OptionsWrapper<HowbotPlayerOptions>(playerOptions), retrieveOptions, token);

      if (!playerResult.IsSuccess)
      {
        logger.LogError($"Failed to create or retrieve player for Guild: {guildId}, Status: {playerResult.Status}");
        throw new InvalidOperationException($"Failed to create or retrieve player for Guild: ${guildId}");
      }

      return playerResult.Player;
    }
    catch (Exception exception)
    {
      logger.LogError(exception, "Failed to create or retrieve player for guild {GuildId}", guildId);
      throw;
    }
  }

  public ValueTask<HowbotPlayer?> GetOrCreatePlayerAsync(SocketInteractionContext context)
  {
    throw new NotImplementedException();
  }

  private ValueTask<HowbotPlayer> CreatePlayerAsync(
    IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> properties,
    CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    Guard.Against.Null(properties, nameof(properties));

    return ValueTask.FromResult(new HowbotPlayer(properties, logger.CastToLoggerClass<HowbotPlayer>()));
  }
}
