using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Application.Interfaces.Lavalink;

namespace Howbot.Infrastructure.Audio.Lavalink.Services;
public class MusicQueueService : IMusicQueueService
{
  public void GetMusicQueue(ulong guildId, ulong voiceChannelId, out List<string> queue, out int currentTrackIndex)
  {
    throw new NotImplementedException();
  }

  public void ToggleShuffle(ulong guildId, ulong voiceChannelId)
  {
    throw new NotImplementedException();
  }
}
