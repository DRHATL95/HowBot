using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Models;
using Howbot.Core.Models.Players;
using JetBrains.Annotations;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;

namespace Howbot.Core.Interfaces;

public interface IMusicService
{
  ValueTask<CommandResponse> PlayTrackBySearchTypeAsync([NotNull] HowbotPlayer player,
    SearchProviderTypes searchProviderType, [NotNull] string searchRequest, [NotNull] IGuildUser user,
    [NotNull] IVoiceState voiceState, [NotNull] ITextChannel textChannel);

  ValueTask<CommandResponse> PauseTrackAsync([NotNull] HowbotPlayer player);

  ValueTask<CommandResponse> ResumeTrackAsync([NotNull] HowbotPlayer player);

  ValueTask<CommandResponse> SkipTrackAsync([NotNull] HowbotPlayer player, [CanBeNull] int? numberOfTracks);

  ValueTask<CommandResponse> SeekTrackAsync([NotNull] HowbotPlayer player, TimeSpan seekPosition);

  ValueTask<CommandResponse> ChangeVolumeAsync([NotNull] HowbotPlayer player, [CanBeNull] int? newVolume);

  ValueTask<CommandResponse> NowPlayingAsync([NotNull] HowbotPlayer player, [NotNull] IGuildUser user,
    [NotNull] ITextChannel textChannel);

  ValueTask<CommandResponse> ApplyAudioFilterAsync([NotNull] HowbotPlayer player, [NotNull] IPlayerFilters filter);

  ValueTask<CommandResponse> GetLyricsFromTrackAsync([NotNull] HowbotPlayer player);

  CommandResponse ToggleShuffle([NotNull] HowbotPlayer player);

  ValueTask<HowbotPlayer> GetPlayerByContextAsync(
    [NotNull] SocketInteractionContext context, bool allowConnect = false, bool requireChannel = true,
    ImmutableArray<IPlayerPrecondition> preconditions = default, bool isDeferred = false,
    CancellationToken cancellationToken = default);

  CommandResponse ToggleTwoFourSeven([NotNull] HowbotPlayer player);

  ValueTask<IEnumerable<string>> GetYoutubeRecommendedVideoId([NotNull] string videoId, int count = 1);
}
