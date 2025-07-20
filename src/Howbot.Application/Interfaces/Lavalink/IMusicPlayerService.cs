using System.Collections.Immutable;
using Discord.Interactions;
using Howbot.Application.Models.Lavalink.Players;
using Lavalink4NET.Players.Preconditions;

namespace Howbot.Application.Interfaces.Lavalink;

public interface IMusicPlayerService
{
  ValueTask<HowbotPlayer?> GetPlayer(ulong guildId);
  ValueTask<HowbotPlayer?> GetPlayer(ulong guildId, string textChannelId);
  ValueTask<HowbotPlayer?> GetPlayer(SocketInteractionContext context, bool allowConnect = false, bool requireChannel = true, ImmutableArray<IPlayerPrecondition> preconditions = default,
    bool isDeferred = false, float initialVolume = 100.0f, CancellationToken cancellationToken = default);
}
