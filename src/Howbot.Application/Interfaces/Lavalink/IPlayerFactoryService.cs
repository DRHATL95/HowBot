using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;
using Howbot.Application.Models.Lavalink.Players;
using Lavalink4NET.Players.Preconditions;

namespace Howbot.Application.Interfaces.Lavalink;
public interface IPlayerFactoryService
{
  ValueTask<HowbotPlayer?> GetOrCreatePlayerAsync(ulong guildId, ulong? voiceChannelId, bool allowConnect = false, 
    bool requireChannel = true, ImmutableArray<IPlayerPrecondition> preconditions = default, CancellationToken token = default);

  ValueTask<HowbotPlayer?> GetOrCreatePlayerAsync(SocketInteractionContext context);
}
