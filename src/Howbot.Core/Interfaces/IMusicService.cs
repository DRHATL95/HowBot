using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Howbot.Core.Models;
using Howbot.Core.Models.Commands;
using Howbot.Core.Models.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;

namespace Howbot.Core.Interfaces;

public interface IMusicService
{
  void Initialize();

  ValueTask<string> GetSessionIdForGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default);

  ValueTask<CommandResponse> PlayTrackBySearchTypeAsync(HowbotPlayer player,
    SearchProviderTypes searchProviderType, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel);

  ValueTask<CommandResponse> PauseTrackAsync(HowbotPlayer player);

  ValueTask<CommandResponse> ResumeTrackAsync(HowbotPlayer player);

  ValueTask<CommandResponse> SkipTrackAsync(HowbotPlayer player, int? numberOfTracks);

  ValueTask<CommandResponse> SeekTrackAsync(HowbotPlayer player, TimeSpan seekPosition);

  ValueTask<CommandResponse> ChangeVolumeAsync(HowbotPlayer player, int newVolume);

  CommandResponse NowPlaying(HowbotPlayer player, IGuildUser user,
    ITextChannel textChannel);

  ValueTask<CommandResponse> ApplyAudioFilterAsync(HowbotPlayer player, IPlayerFilters filter);

  ValueTask<CommandResponse> GetLyricsFromTrackAsync(HowbotPlayer player);

  CommandResponse ToggleShuffle(HowbotPlayer player);

  ValueTask<HowbotPlayer?> GetPlayerByContextAsync(
    SocketInteractionContext context, bool allowConnect = false, bool requireChannel = true,
    ImmutableArray<IPlayerPrecondition> preconditions = default, bool isDeferred = false, int initialVolume = 100,
    CancellationToken cancellationToken = default);

  ValueTask<CommandResponse> JoinVoiceChannelAsync(ulong guildId, ulong voiceChannelId,
    CancellationToken cancellationToken = default);

  CommandResponse GetMusicQueueForServer(HowbotPlayer player);

  ValueTask<HowbotPlayer?> GetPlayerByGuildIdAsync(ulong guildId, CancellationToken cancellationToken = default);
}
