using System.Threading.Tasks;
using Discord;
using Victoria.Player;

namespace Howbot.Core.Interfaces;

public interface IEmbedService
{
  Task<IEmbed> GenerateMusicNowPlayingEmbedAsync(LavaTrack lavaTrack, IGuildUser user, ITextChannel textChannel);
  Task<IEmbed> GenerateMusicNextTrackEmbedAsync();
  Task<IEmbed> GenerateMusicCurrentQueueEmbedAsync();
}
