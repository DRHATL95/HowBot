using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Models;
using Victoria.Player;

namespace Howbot.Core.Interfaces;

public interface IVoiceService : IServiceBase
{
  Task<CommandResponse> JoinVoiceAsync(IGuildUser user, ITextChannel textChannel);

  Task<CommandResponse> LeaveVoiceChannelAsync(IGuildUser user);

  Task InitiateDisconnectLogicAsync(Player<LavaTrack> lavaPlayer, TimeSpan timeSpan);
}
