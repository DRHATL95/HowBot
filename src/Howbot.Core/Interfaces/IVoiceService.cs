using System.Threading.Tasks;
using Discord;
using Howbot.Core.Models;
using Howbot.Core.Models.Commands;

namespace Howbot.Core.Interfaces;

public interface IVoiceService
{
  ValueTask<CommandResponse> JoinVoiceChannelAsync(IGuildUser guildUser, IGuildChannel guildChannel);

  ValueTask<CommandResponse> LeaveVoiceChannelAsync(IGuildUser guildUser, IGuildChannel guildChannel);
}
