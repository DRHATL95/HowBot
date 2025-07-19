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
public interface IMusicInternalService
{
  void Initialize();

  ValueTask<string> GetSessionIdForGuildIdAsync(ulong guildId, CancellationToken ct = default);

  ValueTask<HowbotPlayer?> GetPlayerByGuildIdAsync(ulong guildId, ulong channelId, CancellationToken ct = default);

  ValueTask<HowbotPlayer?> GetPlayerByContextAsync(
      SocketInteractionContext context,
      bool allowConnect = false,
      bool requireChannel = true,
      ImmutableArray<IPlayerPrecondition> preconditions = default,
      bool isDeferred = false,
      int initialVolume = 100,
      CancellationToken ct = default);
}
