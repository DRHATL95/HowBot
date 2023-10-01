using System.Threading.Tasks;
using Discord;
using Howbot.Core.Models;
using JetBrains.Annotations;

namespace Howbot.Core.Interfaces;

// Purpose: Interface for the VoiceService
public interface IVoiceService
{
  /// <summary>
  ///   Joins the requested voice channel for the guild.
  /// </summary>
  /// <param name="guildUser"></param>
  /// <param name="isDeaf"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> JoinVoiceChannelAsync([NotNull] IGuildUser guildUser, bool isDeaf = true);

  /// <summary>
  ///   Leaves the requested voice channel for the guild.
  /// </summary>
  /// <param name="guildUser"></param>
  /// <param name="guildChannel"></param>
  /// <returns></returns>
  ValueTask<CommandResponse> LeaveVoiceChannelAsync([NotNull] IGuildUser guildUser,
    [NotNull] IGuildChannel guildChannel);
}
