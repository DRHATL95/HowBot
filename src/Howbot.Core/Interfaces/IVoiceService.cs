using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET.Players;

namespace Howbot.Core.Interfaces;

public interface IVoiceService : IServiceBase
{

  /// <summary>
  /// Join the command guild user voice channel.
  /// </summary>
  /// <param name="guildUser"></param>
  /// <param name="isDeaf"></param>
  /// <returns></returns>
  [NotNull]
  Task<CommandResponse> JoinVoiceChannelAsync([NotNull] IGuildUser guildUser, bool isDeaf = true);

  /// <summary>
  /// Leave the command guild user voice channel.
  /// </summary>
  /// <param name="guildUser"></param>
  /// <param name="guildChannel"></param>
  /// <returns></returns>
  [NotNull]
  ValueTask<CommandResponse> LeaveVoiceChannelAsync([NotNull] IGuildUser guildUser, [NotNull] IGuildChannel guildChannel);

  /// <summary>
  /// Initiate disconnect by timer with given timespan.
  /// </summary>
  /// <param name="lavalinkPlayer"></param>
  /// <param name="textChannel"></param>
  /// <param name="timeSpan"></param>
  /// <returns></returns>
  [NotNull]
  Task InitiateDisconnectLogicAsync([NotNull] ILavalinkPlayer lavalinkPlayer, TimeSpan timeSpan);
}
