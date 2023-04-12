using System.Threading.Tasks;
using Discord;
using Howbot.Core.Models;
using Victoria.Player;

namespace Howbot.Core.Interfaces;

public interface IEmbedService : IServiceBase
{
  IEmbed CreateEmbed(EmbedOptions embedOptions);

  Task<IEmbed> GenerateMusicNowPlayingEmbedAsync(LavaTrack lavaTrack, IGuildUser user, ITextChannel textChannel);

  Task<IEmbed> GenerateMusicNextTrackEmbedAsync(Vueue<LavaTrack> queue);

  Task<IEmbed> GenerateMusicCurrentQueueEmbedAsync(Vueue<LavaTrack> queue);
}
