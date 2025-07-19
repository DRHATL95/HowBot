using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Howbot.Application.Models.Discord.Commands;
using Howbot.Application.Models.Lavalink;
using Howbot.Application.Models.Lavalink.Players;

namespace Howbot.Application.Interfaces.Lavalink;
public interface IMusicDiscordService
{
  CommandResponse NowPlayingEmbed(HowbotPlayer player, IGuildUser user, ITextChannel channel);

  ValueTask<MusicCommandResult> PlayWithContextAsync(
      HowbotPlayer player,
      string searchRequest,
      IGuildUser user,
      IVoiceState voiceState,
      ITextChannel textChannel);
}
