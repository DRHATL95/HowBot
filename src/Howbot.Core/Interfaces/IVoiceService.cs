using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET.Players;

namespace Howbot.Core.Interfaces;

public interface IVoiceService
{

  [NotNull]
  Task<CommandResponse> JoinVoiceChannelAsync([NotNull] IGuildUser guildUser, bool isDeaf = true);

  [NotNull]
  ValueTask<CommandResponse> LeaveVoiceChannelAsync([NotNull] IGuildUser guildUser, [NotNull] IGuildChannel guildChannel);

  [NotNull]
  Task InitiateDisconnectLogicAsync([NotNull] ILavalinkPlayer lavalinkPlayer, TimeSpan timeSpan);

}
