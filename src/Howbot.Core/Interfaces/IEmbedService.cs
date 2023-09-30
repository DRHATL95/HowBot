using System;
using System.Threading.Tasks;
using Discord;
using Howbot.Core.Models;
using JetBrains.Annotations;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Interfaces;

public interface IEmbedService
{
  void Initialize();

  [NotNull]
  IEmbed CreateEmbed([NotNull] EmbedOptions embedOptions);

  [NotNull]
  ValueTask<IEmbed> GenerateMusicNowPlayingEmbedAsync([NotNull] LavalinkTrack queueItem, [NotNull] IGuildUser user,
    [NotNull] ITextChannel textChannel, TimeSpan? position);

  [NotNull]
  ValueTask<IEmbed> GenerateMusicNextTrackEmbedAsync([NotNull] ITrackQueue queue);

  [NotNull]
  ValueTask<IEmbed> GenerateMusicCurrentQueueEmbedAsync([NotNull] ITrackQueue queue);
}
