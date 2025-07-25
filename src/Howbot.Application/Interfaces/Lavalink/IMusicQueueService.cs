using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howbot.Application.Interfaces.Lavalink;
public interface IMusicQueueService
{
  void GetMusicQueue(ulong guildId, ulong voiceChannelId, out List<string> queue, out int currentTrackIndex);
  void ToggleShuffle(ulong guildId, ulong voiceChannelId);
}
