using Discord;
using Howbot.Application.Models.Discord.Commands;

namespace Howbot.Application.Interfaces.Lavalink;

public interface IVoiceService
{
  ValueTask<CommandResponse> JoinVoiceChannelAsync(IGuildUser guildUser, IGuildChannel guildChannel);

  ValueTask<CommandResponse> LeaveVoiceChannelAsync(IGuildUser guildUser, IGuildChannel guildChannel);
}
