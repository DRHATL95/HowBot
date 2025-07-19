using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Interactions;
using Howbot.Application.Models.Lavalink.Players;

namespace Howbot.Application.Interfaces.Lavalink;
public interface IPlayerFactoryService
{
  ValueTask<HowbotPlayer?> GetOrCreatePlayerAsync(ulong guildId, ulong voiceChannelId, CancellationToken token = default);
  ValueTask<HowbotPlayer?> GetOrCreatePlayerAsync(SocketInteractionContext context);
}
