using System.Collections.Immutable;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Application.Constants;
using Howbot.Application.Interfaces.Lavalink;
using Howbot.Application.Models.Lavalink.Players;
using Howbot.Infrastructure.Data;
using Howbot.SharedKernel;
using Lavalink4NET;
using Lavalink4NET.Clients;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Howbot.Infrastructure.Audio.Lavalink.Services;

public class MusicPlayerService(IAudioService audioService, IServiceProvider serviceProvider, ILoggerAdapter<MusicPlayerService> logger) : IMusicPlayerService
{
  public async ValueTask<HowbotPlayer?> GetPlayer(ulong guildId)
  {
    try
    {
      var playerResult = await audioService.Players.GetPlayerAsync<HowbotPlayer>(guildId);
      if (playerResult is not null)
      {
        return playerResult;
      }

      logger.LogError($"Player {guildId} not found");
      return null;
    }
    catch (Exception exception)
    {
      logger.LogError(exception, $"Player {guildId} not found");
      return null;
    }
  }

  public async ValueTask<HowbotPlayer?> GetPlayer(SocketInteractionContext context, bool allowConnect = false, bool requireChannel = true, ImmutableArray<IPlayerPrecondition> preconditions = default,
    bool isDeferred = false, float initialVolume = 100.0f, CancellationToken cancellationToken = default)
  {
    if (context.User is not SocketGuildUser || context.Channel is not ITextChannel)
    {
      logger.LogError("Unable to get player, incorrect user type or channel type.");
      return null;
    }
    
    var guildId =  context.Guild.Id;
    var voiceChannelId = context.Channel.Id;

    using var scope = serviceProvider.CreateScope();
    var database = serviceProvider.GetRequiredService<EfRepository>();
    var guild = database.GetGuildByGuildId(guildId);

    var volume = guild?.Volume ?? BotDefaults.DefaultVolume;

    var options = new HowbotPlayerOptions { InitialVolume = volume };

    var playerRetrieveOptions = new PlayerRetrieveOptions(
      allowConnect ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None,
      requireChannel ? MemberVoiceStateBehavior.RequireSame : MemberVoiceStateBehavior.Ignore,
      preconditions);

    var playerGetResult = await audioService.Players.RetrieveAsync<HowbotPlayer, HowbotPlayerOptions>(guildId, 
      voiceChannelId, CreatePlayer, new OptionsWrapper<HowbotPlayerOptions>(options), playerRetrieveOptions, cancellationToken);
    
    return playerGetResult.IsSuccess ?  playerGetResult.Player : null;
  }

  private ValueTask<HowbotPlayer> CreatePlayer(IPlayerProperties<HowbotPlayer, HowbotPlayerOptions> playerOptions,
    CancellationToken cancellationToken)
  {
    return ValueTask.FromResult(new HowbotPlayer(playerOptions, logger.CastToLoggerClass<HowbotPlayer>()));
  }
}
